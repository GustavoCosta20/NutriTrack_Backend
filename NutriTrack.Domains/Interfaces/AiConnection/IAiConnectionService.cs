namespace NutriTrack_Domains.Interfaces.AiConnection
{
    public interface IAiConnectionService
    {
        Task<string> AnalisarRefeicao(string prompt);

    }
}
