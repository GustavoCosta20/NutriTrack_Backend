using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.NutritionCalculator;
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
        private readonly INutritionCalculatorService _nutritionCalculator;

        public RegisterAndLoginServ(IRepository<Users> usersRepository, IConfiguration configuration, INutritionCalculatorService nutritionCalculator)
        {
            _usersRepository = usersRepository;
            _configuration = configuration;
            _nutritionCalculator = nutritionCalculator;
        }

        public async Task RegisterUser(RegisterUserDto info)
        {
            try
            {
                var existingUser = await _usersRepository.FirstOrDefaultAsync(u => u.Email == info.Email);

                if (existingUser != null)
                    throw new Exception("E-mail já cadastrado.");

                var userData = new Users
                {
                    NomeCompleto = info.NomeCompleto,
                    AlturaEmCm = info.AlturaEmCm,
                    CriadoEm = DateTime.UtcNow.AddHours(-3),
                    DataNascimento = info.DataNascimento,
                    Email = info.Email,
                    Genero = info.Genero,
                    Id = Guid.NewGuid(),
                    NivelDeAtividade = info.NivelDeAtividade,
                    Objetivo = info.Objetivo,
                    PesoEmKg = info.PesoEmKg,
                    Senha = BCrypt.Net.BCrypt.HashPassword(info.Senha)
                };

                _nutritionCalculator.CalcularPlanoNutricional(userData);

                await _usersRepository.AddAsync(userData);
            }
            catch (Exception E)
            {
                throw new Exception(E.Message);
            }
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
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<ProfileDataDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _usersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            var profileData = new ProfileDataDto
            {
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                DataNascimento = user.DataNascimento,
                AlturaEmCm = user.AlturaEmCm,
                PesoEmKg = user.PesoEmKg,
                Genero = user.Genero,
                NivelDeAtividade = user.NivelDeAtividade,
                Objetivo = user.Objetivo
            };

            return profileData;
        }

        public async Task UpdateUserProfileAsync(UpdateProfileDto dto)
        {
            var user = await _usersRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            user.NomeCompleto = dto.NomeCompleto;
            user.DataNascimento = dto.DataNascimento;
            user.AlturaEmCm = dto.AlturaEmCm;
            user.PesoEmKg = dto.PesoEmKg;
            user.Genero = dto.Genero;
            user.NivelDeAtividade = dto.NivelDeAtividade;
            user.Objetivo = dto.Objetivo;
            user.AtualizadoEm = DateTime.UtcNow.AddHours(-3);

            _nutritionCalculator.CalcularPlanoNutricional(user);

            await _usersRepository.UpdateAsync(user);
        }
    }
}
