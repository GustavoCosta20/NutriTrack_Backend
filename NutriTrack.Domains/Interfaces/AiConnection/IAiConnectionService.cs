using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Domains.Interfaces.AiConnection
{
    public interface IAiConnectionService
    {

        Task<string> GeminiConnection(string pergunta);

    }
}
