using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.SnackService;
using System.Security.Claims;

namespace NutriTrack_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RefeicaoController : ControllerBase
    {
        private readonly IRefeicaoService _refeicaoService;

        public RefeicaoController(IRefeicaoService refeicaoService)
        {
            _refeicaoService = refeicaoService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarRefeicao([FromBody] CriarRefeicaoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.DescricaoRefeicao))
                {
                    return BadRequest(new
                    {
                        sucesso = false,
                        mensagem = "A descrição da refeição é obrigatória."
                    });
                }

                var usuarioId = ObterUsuarioIdDoToken();

                var refeicao = await _refeicaoService.ProcessarECriarRefeicao(
                    usuarioId,
                    request.DescricaoRefeicao,
                    request.NomeRefeicao ?? "Refeição"
                );

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Refeição registrada com sucesso!",
                    refeicao
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterRefeicoes([FromQuery] DateOnly? data = null)
        {
            try
            {
                var usuarioId = ObterUsuarioIdDoToken();
                var refeicoes = await _refeicaoService.ObterRefeicoesDoUsuario(usuarioId, data);
                return Ok(refeicoes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("hoje")]
        public async Task<IActionResult> ObterRefeicoesDeHoje()
        {
            try
            {
                var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);

                var usuarioId = ObterUsuarioIdDoToken();
                var hoje = DateOnly.FromDateTime(dataHoraBrasilia);
                var refeicoes = await _refeicaoService.ObterRefeicoesDoUsuario(usuarioId, hoje);

                var totais = new TotaisDiarios
                {
                    Calorias = refeicoes.Sum(r => r.TotalCalorias),
                    Proteinas = refeicoes.Sum(r => r.TotalProteinas),
                    Carboidratos = refeicoes.Sum(r => r.TotalCarboidratos),
                    Gorduras = refeicoes.Sum(r => r.TotalGorduras)
                };

                var response = new RefeicoesDoHojeResponse
                {
                    Refeicoes = refeicoes,
                    Totais = totais
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterRefeicaoPorId(Guid id)
        {
            try
            {
                var refeicao = await _refeicaoService.ObterRefeicaoPorId(id);
                return Ok(refeicao);
            }
            catch (Exception ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        private Guid ObterUsuarioIdDoToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            return usuarioId;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarRefeicao(Guid id, [FromBody] CriarRefeicaoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.DescricaoRefeicao))
                {
                    return BadRequest(new
                    {
                        sucesso = false,
                        mensagem = "A descrição da refeição é obrigatória."
                    });
                }

                var usuarioId = ObterUsuarioIdDoToken();

                var refeicao = await _refeicaoService.AtualizarRefeicao(id, usuarioId, request.DescricaoRefeicao, request.NomeRefeicao ?? "");

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Refeição atualizada com sucesso!",
                    refeicao
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = ex.Message
                });
            }
        }
    }
}
