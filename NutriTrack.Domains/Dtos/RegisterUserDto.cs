using NutriTrack_Domains.Enums;

namespace NutriTrack_Domains.Dtos
{
    public class RegisterUserDto
    {
        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateOnly DataNascimento { get; set; }
        public double AlturaEmCm { get; set; }
        public double PesoEmKg { get; set; }
        public EnumGenero Genero { get; set; }
        public EnumNivelDeAtividade NivelAtividade { get; set; }
        public EnumObjetivo Objetivo { get; set; }
    }
}
