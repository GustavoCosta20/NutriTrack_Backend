using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.AiConnection;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Tables.UsersTb;
using System.Security.Claims;

namespace NutriTrack_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatIaController : ControllerBase
    {
        private readonly IAiConnectionService _aiService;
        private readonly IRepository<Users> _userRepository;

        public ChatIaController(IAiConnectionService aiService, IRepository<Users> userRepository)
        {
            _aiService = aiService;
            _userRepository = userRepository;
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

                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized("Token inválido ou ID do usuário não encontrado.");
                }

                var usuario = await _userRepository.GetByIdAsync(userId);


                var resposta = await _aiService.ConversarSobreNutricao(request.Mensagem, usuario);

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