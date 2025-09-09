using Microsoft.AspNetCore.Mvc;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.UserInterfaces;

namespace NutriTrack_Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IRegisterAndLoginServ _registerAndLoginServ;

        public UserController(IRegisterAndLoginServ registerAndLoginServ)
        {
            _registerAndLoginServ = registerAndLoginServ;
        }

        [HttpPost("register")]
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
        public async Task<IActionResult> LoginUser(UserDataLoginDto info)
        {
            try
            {
                var token = await _registerAndLoginServ.LoginUser(info);
                return Ok(token);
            }
            catch (Exception E)
            {
                throw new Exception(E.Message);
            }
        }
    }
}
