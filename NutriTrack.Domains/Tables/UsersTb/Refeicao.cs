using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Tables.UsersTb
{
    [Table("TbRefeicao")]
    public class Refeicao
    {
        [Key]
        public Guid Id { get; set; }

        // permite que EF Core carregue o objeto Usuario relacionado
        [ForeignKey("UsuarioId")]
        public Users Usuario { get; set; }

        [Required]
        public Guid UsuarioId { get; set; } // Chave estrangeira para a tabela Usuarios

        [Required]
        [MaxLength(100)]
        public string NomeRef { get; set; }

        public DateOnly Data { get; set; }

        // Uma Refeicao multiplos AlimentosConsumidos
        public virtual ICollection<AlimentosConsumido> AlimentosConsumidos { get; set; }

        // Boa prática: inicializar coleções no construtor para evitar erros de referência nula
        public Refeicao()
        {
            AlimentosConsumidos = new HashSet<AlimentosConsumido>();
        }
    }
}
