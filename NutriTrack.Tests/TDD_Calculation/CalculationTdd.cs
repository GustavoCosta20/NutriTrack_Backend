using NutriTrack_Domains.Enums;
using NutriTrack_Domains.Tables.UsersTb;
using NutriTrack_Services.CalculatorService;
using Xunit;

namespace NutriTrack_Tests.TDD_Calculation
{
    //calcula metas corretamente HOMEM
    public class NutritionCalculatorServiceTests
    {
        [Fact]
        public void CalculaNutri_HomemSedentarioPercaGordura()
        {

            var calculator = new NutritionCalculatorService();

            var usuario = new Users
            {
                Genero = EnumGenero.Masculino,
                PesoEmKg = 80,
                AlturaEmCm = 180,
                DataNascimento = new DateOnly(1995, 1, 1),
                NivelDeAtividade = EnumNivelDeAtividade.Sedentario,
                Objetivo = EnumObjetivo.PerderGordura
            };

            calculator.CalcularPlanoNutricional(usuario);

            Assert.Equal(1736, usuario.MetaCalorias);
            Assert.Equal(160, usuario.MetaProteinas);
            Assert.Equal(166, usuario.MetaCarboidratos);
            Assert.Equal(48, usuario.MetaGorduras);
        }

        //calcula metas corretamente MULHER
        [Fact]
        public void CalculaNutri_MulherAtivaGanharMassa()
        {
            // --- ARRANGE ---
            var calculator = new NutritionCalculatorService();
            var usuario = new Users
            {
                Genero = EnumGenero.Feminino,
                PesoEmKg = 60,
                AlturaEmCm = 165,
                DataNascimento = new DateOnly(2000, 1, 1),
                NivelDeAtividade = EnumNivelDeAtividade.AtividadeAlta,
                Objetivo = EnumObjetivo.GanharMassa
            };

            calculator.CalcularPlanoNutricional(usuario);

            Assert.Equal(2621, usuario.MetaCalorias);
            Assert.Equal(120, usuario.MetaProteinas);
            Assert.Equal(371, usuario.MetaCarboidratos);
            Assert.Equal(73, usuario.MetaGorduras);
        }
    }
}
