using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
