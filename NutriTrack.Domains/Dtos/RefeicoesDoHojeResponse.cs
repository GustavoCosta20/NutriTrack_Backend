using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Dtos
{
    public class RefeicoesDoHojeResponse
    {
        public List<RefeicaoDto> Refeicoes { get; set; }
        public TotaisDiarios Totais { get; set; }
    }
}
