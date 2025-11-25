using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.UserInterfaces;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Tables.UsersTb;
using System.Security.Claims;

namespace NutriTrack_Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IRegisterAndLoginServ _registerAndLoginServ;
        private readonly IRepository<Users> _userRepository;

        public UserController(IRegisterAndLoginServ registerAndLoginServ, IRepository<Users> userRepository)
        {
            _registerAndLoginServ = registerAndLoginServ;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(RegisterUserDto info)
        {
            try
            {
                await _registerAndLoginServ.RegisterUser(info);
                return Ok();
            }
            catch (Exception E)
            {
                throw new Exception(E.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser(UserDataLoginDto info)
        {
            try
            {
                var token = await _registerAndLoginServ.LoginUser(info);
                return Ok(new { Token = token });
            }
            catch (Exception E)
            {
                throw new Exception(E.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized("Token inválido ou ID do usuário não encontrado.");
                }

                var usuario = await _userRepository.GetByIdAsync(userId);
                if (usuario == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                var userProfile = new UserProfileDto
                {
                    NomeCompleto = usuario.NomeCompleto,
                    Email = usuario.Email,
                    DataNascimento = usuario.DataNascimento,
                    AlturaEmCm = usuario.AlturaEmCm,
                    PesoEmKg = usuario.PesoEmKg,
                    Genero = usuario.Genero,
                    NivelDeAtividade = usuario.NivelDeAtividade,
                    Objetivo = usuario.Objetivo.ToString(),
                    MetaCalorias = usuario.MetaCalorias,
                    MetaProteinas = usuario.MetaProteinas,
                    MetaCarboidratos = usuario.MetaCarboidratos,
                    MetaGorduras = usuario.MetaGorduras
                };

                return Ok(userProfile);
            }
            catch (Exception E)
            {
                return StatusCode(500, new { Erro = "Ocorreu um erro ao buscar os dados do perfil." });
            }
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUserProfile(UpdateProfileDto info)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized("Token inválido ou ID do usuário não encontrado.");
                }
                info.UserId = userId;
                await _registerAndLoginServ.UpdateUserProfileAsync(info);
                return Ok();
            }
            catch (Exception E)
            {
                throw new Exception(E.Message);
            }
        }
    }
}

