using NutriTrack_Domains.Enums;

namespace NutriTrack_Domains.Dtos
{
    public class UpdateProfileDto
    {
        public Guid UserId { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public DateOnly DataNascimento { get; set; }
        public double AlturaEmCm { get; set; }
        public double PesoEmKg { get; set; }
        public EnumGenero Genero { get; set; }
        public EnumNivelDeAtividade NivelDeAtividade { get; set; }
        public EnumObjetivo Objetivo { get; set; }
    }
}
