using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Dtos
{
    public class AlimentoConsumidoDto
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; }
        public double Quantidade { get; set; }
        public string Unidade { get; set; }
        public double Calorias { get; set; }
        public double Proteinas { get; set; }
        public double Carboidratos { get; set; }
        public double Gorduras { get; set; }
    }
}
