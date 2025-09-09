using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Enums;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Tables.UsersTb;
using NutriTrack_Services.UserServices;

namespace NutriTrack_Tests.TDD
{
    public class RegistroServiceTests
    {
        [Fact]
        public async Task RegistrarAsync_QuandoEmailJaExiste_DeveRetornarFalha()
        {
            var dataNascimento = new DateOnly(1995, 10, 20);
            var mockRepo = new Mock<NutriTrack_Domains.Interfaces.Repository.IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();

            var dto = new RegisterUserDto
            {
                NomeCompleto = "Gustavo Costa",
                Email = "gusta@email.com",
                DataNascimento = dataNascimento,
                Genero = EnumGenero.Masculino,
                Senha = "senha123",
                AlturaEmCm = 180,
                PesoEmKg = 72,
                NivelAtividade = EnumNivelDeAtividade.AtividadeModerada,
                Objetivo = EnumObjetivo.GanharMassa,
            };

            mockRepo.Setup(r => r.AddAsync(It.IsAny<Users>())).Returns(Task.CompletedTask);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object);

            var exception = await Record.ExceptionAsync(() => service.RegisterUser(dto));

            Assert.Null(exception);
        }
    }
}