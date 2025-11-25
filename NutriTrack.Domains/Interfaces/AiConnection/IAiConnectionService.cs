using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Tables.UsersTb;

namespace NutriTrack_Domains.Interfaces.AiConnection
{
    public interface IAiConnectionService
    {
        Task<string> AnalisarRefeicao(string prompt);
        Task<string> ConversarSobreNutricao(string mensagemUsuario, Users perfil);
    }
}
