using NutriTrack_Domains.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Dtos
{
    public class ProfileDataDto
    {
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
