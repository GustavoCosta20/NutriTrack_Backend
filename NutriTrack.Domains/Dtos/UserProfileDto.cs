using NutriTrack_Domains.Enums;

namespace NutriTrack_Domains.Dtos
{
    public class UserProfileDto
    {
        public string NomeCompleto { get; set; }
        public string Objetivo { get; set; } 
        public int MetaCalorias { get; set; }
        public int MetaProteinas { get; set; }
        public int MetaCarboidratos { get; set; }
        public int MetaGorduras { get; set; }
        public string Email { get; set; }
        public DateOnly DataNascimento { get; set; }
        public double AlturaEmCm { get; set; }
        public double PesoEmKg { get; set; }
        public EnumGenero Genero { get; set; }
        public EnumNivelDeAtividade NivelDeAtividade { get; set; }
    }
}
