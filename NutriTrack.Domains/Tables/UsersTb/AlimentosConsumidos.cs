using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Tables.UsersTb
{
    [Table("TbAlimentosConsumidos")]
    public class AlimentosConsumido
    {
        [Key]
        public Guid Id { get; set; }

        // Propriedade de Navegação: permite acessar a Refeicao a qual este alimento pertence
        [ForeignKey("RefeicaoId")]
        public Refeicao Refeicao { get; set; }

        [Required]
        public Guid RefeicaoId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Descricao { get; set; } // Armazena texto que usuário digitou pra api

        public double Quantidade { get; set; }

        [MaxLength(50)]
        public string Unidade { get; set; } //g, ml, fatia, unidade

        // Armazena resposta API Gemini
        public double Calorias { get; set; }
        public double Proteinas { get; set; }
        public double Carboidratos { get; set; }
        public double Gorduras { get; set; }
    }
}
