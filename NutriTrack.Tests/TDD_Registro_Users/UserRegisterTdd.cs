using Microsoft.Extensions.Configuration;
using Moq;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Enums;
using NutriTrack_Domains.Interfaces.NutritionCalculator;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Tables.UsersTb;
using NutriTrack_Services.UserServices;
using System.Linq.Expressions;
using Xunit;

namespace NutriTrack_Tests.TDD
{
    public class RegistroServiceTests
    {
        [Fact]
        public async Task RegisterUser_QuandoDadosValidos_DeveRegistrarSemErros()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync((Users)null);

            mockRepo.Setup(r => r.AddAsync(It.IsAny<Users>()))
                    .Returns(Task.CompletedTask);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            var dto = new RegisterUserDto
            {
                NomeCompleto = "Gustavo Costa",
                Email = "gusta@email.com",
                DataNascimento = new DateOnly(1995, 10, 20),
                Genero = EnumGenero.Masculino,
                Senha = "senha123",
                AlturaEmCm = 180,
                PesoEmKg = 72,
                NivelDeAtividade = EnumNivelDeAtividade.AtividadeModerada,
                Objetivo = EnumObjetivo.GanharMassa
            };

            var ex = await Record.ExceptionAsync(() => service.RegisterUser(dto));

            Assert.Null(ex);
        }

        [Fact]
        public async Task RegisterUser_QuandoEmailExistente_DeveLancarExcecao()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync(new Users());

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            var dto = new RegisterUserDto
            {
                NomeCompleto = "Gustavo Costa",
                Email = "teste@email.com",
                DataNascimento = new DateOnly(1995, 10, 20),
                Genero = EnumGenero.Masculino,
                Senha = "senha123",
                AlturaEmCm = 180,
                PesoEmKg = 72,
                NivelDeAtividade = EnumNivelDeAtividade.AtividadeModerada,
                Objetivo = EnumObjetivo.GanharMassa
            };

            await Assert.ThrowsAsync<Exception>(() => service.RegisterUser(dto));
        }

        [Fact]
        public async Task LoginUser_QuandoUsuarioNaoExiste_DeveLancarExcecao()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync((Users)null);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            await Assert.ThrowsAsync<Exception>(() => service.LoginUser(
                new UserDataLoginDto { Email = "x", Senha = "y" }
            ));
        }

        [Fact]
        public async Task LoginUser_QuandoSenhaIncorreta_DeveLancarExcecao()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            var usuario = new Users
            {
                Email = "teste@email.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("senha_correta")
            };

            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync(usuario);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            await Assert.ThrowsAsync<Exception>(() => service.LoginUser(
                new UserDataLoginDto { Email = usuario.Email, Senha = "senha_errada" }
            ));
        }

        [Fact]
        public async Task LoginUser_QuandoCredenciaisValidas_DeveRetornarToken()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            var usuario = new Users
            {
                Id = Guid.NewGuid(),
                Email = "teste@email.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("123456")
            };

            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync(usuario);

            mockJWT.Setup(x => x["JwtSettings:SecretKey"]).Returns("12345678901234567890123456789012"); // 32 chars
            mockJWT.Setup(x => x["JwtSettings:Issuer"]).Returns("issuer");
            mockJWT.Setup(x => x["JwtSettings:Audience"]).Returns("aud");

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            var token = await service.LoginUser(
                new UserDataLoginDto { Email = usuario.Email, Senha = "123456" }
            );

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task GetUserProfile_QuandoUsuarioExiste_DeveRetornarDados()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            var usuario = new Users
            {
                Id = Guid.NewGuid(),
                NomeCompleto = "Gustavo",
                Email = "g@x.com",
                PesoEmKg = 70,
                AlturaEmCm = 180,
                Genero = EnumGenero.Masculino
            };

            mockRepo.Setup(r => r.GetByIdAsync(usuario.Id))
                    .ReturnsAsync(usuario);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            var result = await service.GetUserProfileAsync(usuario.Id);

            Assert.Equal("Gustavo", result.NomeCompleto);
            Assert.Equal("g@x.com", result.Email);
        }

        [Fact]
        public async Task GetUserProfile_QuandoNaoExiste_DeveLancarExcecao()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Users)null);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            await Assert.ThrowsAsync<Exception>(() => service.GetUserProfileAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateUserProfile_QuandoUsuarioExiste_DeveAtualizar()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            var usuario = new Users
            {
                Id = Guid.NewGuid(),
                NomeCompleto = "Antigo Nome"
            };

            mockRepo.Setup(r => r.GetByIdAsync(usuario.Id))
                    .ReturnsAsync(usuario);

            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Users>()))
                    .Returns(Task.CompletedTask);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            var dto = new UpdateProfileDto
            {
                UserId = usuario.Id,
                NomeCompleto = "Novo Nome"
            };

            await service.UpdateUserProfileAsync(dto);

            Assert.Equal("Novo Nome", usuario.NomeCompleto);
        }

        [Fact]
        public async Task UpdateUserProfile_QuandoNaoExiste_DeveLancarExcecao()
        {
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();
            var mockCalc = new Mock<INutritionCalculatorService>();

            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((Users)null);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object, mockCalc.Object);

            await Assert.ThrowsAsync<Exception>(() => service.UpdateUserProfileAsync(
                new UpdateProfileDto { UserId = Guid.NewGuid() }
            ));
        }

    }
}