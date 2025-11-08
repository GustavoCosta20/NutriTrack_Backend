using NutriTrack_Domains.Dtos;

namespace NutriTrack_Domains.Interfaces.SnackService
{
    public interface IRefeicaoService
    {
        Task<RefeicaoDto> ProcessarECriarRefeicao(Guid usuarioId, string descricaoRefeicao, string nomeRefeicao);
        Task<List<RefeicaoDto>> ObterRefeicoesDoUsuario(Guid usuarioId, DateOnly? data = null);
        Task<RefeicaoDto> ObterRefeicaoPorId(Guid refeicaoId);
    }
}
