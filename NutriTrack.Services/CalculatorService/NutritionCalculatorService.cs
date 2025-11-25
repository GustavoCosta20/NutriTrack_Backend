using NutriTrack_Domains.Enums;
using NutriTrack_Domains.Interfaces.NutritionCalculator;
using NutriTrack_Domains.Tables.UsersTb;
using System;

namespace NutriTrack_Services.CalculatorService
{
    public class NutritionCalculatorService : INutritionCalculatorService
    {
        public void CalcularPlanoNutricional(Users usuario)
        {
            int idade = CalcularIdade(usuario.DataNascimento);

            // TMB
            double tmb;
            if (usuario.Genero == EnumGenero.Masculino)
            {
                tmb = (10 * usuario.PesoEmKg) + (6.25 * usuario.AlturaEmCm) - (5 * idade) + 5;
            }
            else // Feminino ou Outro
            {
                tmb = (10 * usuario.PesoEmKg) + (6.25 * usuario.AlturaEmCm) - (5 * idade) - 161;
            }

            // Gasto Total Diário
            double fatorAtividade = GetFatorAtividade(usuario.NivelDeAtividade);
            double tdee = tmb * fatorAtividade;

            // calorias com base no objetivo
            int metaCalorias = AjustarCaloriasPorObjetivo(tdee, usuario.Objetivo);

            // Calcula Macros
            int metaProteinas = (int)Math.Round(usuario.PesoEmKg * 2);
            double caloriasProteina = metaProteinas * 4;

            double caloriasGordura = metaCalorias * 0.25;
            int metaGorduras = (int)Math.Round(caloriasGordura / 9);

            double caloriasCarboidratos = metaCalorias - caloriasProteina - (metaGorduras * 9);
            int metaCarboidratos = (int)Math.Round(caloriasCarboidratos / 4);

            usuario.MetaCalorias = metaCalorias;
            usuario.MetaProteinas = metaProteinas;
            usuario.MetaCarboidratos = metaCarboidratos;
            usuario.MetaGorduras = metaGorduras;
        }

        private int CalcularIdade(DateOnly dataNascimento)
        {
            var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);
            var hoje = DateOnly.FromDateTime(dataHoraBrasilia);
            int idade = hoje.Year - dataNascimento.Year;
            if (hoje.DayOfYear < dataNascimento.DayOfYear)
            {
                idade--;
            }
            return idade;
        }

        private double GetFatorAtividade(EnumNivelDeAtividade nivel)
        {
            return nivel switch
            {
                EnumNivelDeAtividade.Sedentario => 1.2,
                EnumNivelDeAtividade.AtividadeLeve => 1.375,
                EnumNivelDeAtividade.AtividadeModerada => 1.55,
                EnumNivelDeAtividade.AtividadeAlta => 1.725,
                _ => 1.2
            };
        }

        private int AjustarCaloriasPorObjetivo(double tdee, EnumObjetivo objetivo)
        {
            double caloriasAjustadas = objetivo switch
            {
                EnumObjetivo.PerderGordura => tdee - 400,
                EnumObjetivo.GanharMassa => tdee + 300,
                EnumObjetivo.TrocarGordura => tdee,
                _ => tdee
            };

            return (int)Math.Round(caloriasAjustadas);
        }

    }
}