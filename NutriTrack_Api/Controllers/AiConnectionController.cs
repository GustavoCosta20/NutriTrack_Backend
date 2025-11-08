using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriTrack_Domains.Interfaces.AiConnection;

namespace NutriTrack_Api.Controllers
{
    [Route("api/ai")]
    [ApiController]
    [Authorize]
    public class AiConnectionController : Controller
    {
        
        private readonly IAiConnectionService _aiConnectionService;

        public AiConnectionController(IAiConnectionService aiConnectionService)
        {
            _aiConnectionService = aiConnectionService;
        }

        [HttpGet("connection")]
        [Authorize]
        public async Task<IActionResult> GeminiIntegration([FromQuery] string pergunta)
        {
            try
            {
                //string retornoIa = await _aiConnectionService.GeminiConnection(pergunta);
                return Ok(/*retornoIa.Replace("##","\n\n##")*/);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);

            }
        }

    }
}
