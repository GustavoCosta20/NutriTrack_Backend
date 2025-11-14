using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Dtos
{
    public class ChatIaRequest
    {
        public string Mensagem { get; set; }
    }

    public class ChatIaResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public string Resposta { get; set; }
    }
}
