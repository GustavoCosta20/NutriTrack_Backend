using NutriTrack_Domains.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Tables.UsersTb
{
    [Table("TbUsers")]
    public class Users
    {
        [Key]
        public Guid Id { get; set; }

        public string NomeCompleto { get; set; }

        public string Email { get; set; }

        public string Senha { get; set; }

        public DateOnly DataNascimento { get; set; }

        public double AlturaEmCm { get; set; }

        public double PesoEmKg { get; set; }

        public DateTime CriadoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }

        public EnumGenero Genero { get; set; }

        public EnumNivelDeAtividade NivelDeAtividade { get; set; }

        public EnumObjetivo Objetivo { get; set; }

    }
}
