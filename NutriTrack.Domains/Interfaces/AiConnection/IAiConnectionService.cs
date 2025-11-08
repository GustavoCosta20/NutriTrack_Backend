namespace NutriTrack_Domains.Interfaces.AiConnection
{
    public interface IAiConnectionService
    {
        //Task<string> GeminiConnection(string pergunta);
        Task<string> AnalisarRefeicao(string prompt);

    }
}
