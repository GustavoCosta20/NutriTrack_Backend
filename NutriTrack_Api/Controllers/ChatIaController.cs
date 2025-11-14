using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.AiConnection;

namespace NutriTrack_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatIaController : ControllerBase
    {
        private readonly IAiConnectionService _aiService;

        public ChatIaController(IAiConnectionService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("conversar")]
        public async Task<IActionResult> Conversar([FromBody] ChatIaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Mensagem))
                {
                    return BadRequest(new ChatIaResponse
                    {
                        Sucesso = false,
                        Mensagem = "A mensagem não pode estar vazia."
                    });
                }

                var resposta = await _aiService.ConversarSobreNutricao(request.Mensagem);

                return Ok(new ChatIaResponse
                {
                    Sucesso = true,
                    Mensagem = "Resposta gerada com sucesso",
                    Resposta = resposta
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ChatIaResponse
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                });
            }
        }
    }
}