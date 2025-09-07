using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Interfaces.UserInterfaces;
using NutriTrack_Domains.Tables.UsersTb;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NutriTrack_Services.UserServices
{
    public class RegisterAndLoginServ : IRegisterAndLoginServ
    {
        private readonly IRepository<Users> _usersRepository;
        private readonly IConfiguration _configuration;

        public RegisterAndLoginServ(IRepository<Users> usersRepository, IConfiguration configuration)
        {
            _usersRepository = usersRepository;
            _configuration = configuration;
        }

        public async Task RegisterUser(RegisterUserDto info)
        {
            //await _usersRepository.GetByIdAsync(id);
        }

        public async Task<string> LoginUser(UserDataLoginDto info)
        {
            var user = await _usersRepository.FirstOrDefaultAsync(a => a.Email == info.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(info.Senha, user.Senha))
            {
                throw new Exception("Credênciais inválidas!");
            }

            var token = GerarToken(user);

            return token;
        }

        private string GerarToken(Users usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Pega a chave secreta do appsettings.json
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
