using Moq;
using Xunit;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Tables.UsersTb;
using NutriTrack_Services.UserServices;
using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;

namespace NutriTrack_Tests.TDD_Login_Users
{
    public class UserLoginTdd
    {
        // Cenário 1: Teste de falha quando o e-mail não existe no banco de dados.
        [Fact]
        public async Task LoginUser_QuandoUsuarioNaoEncontrado_DeveLancarExcecao()
        {
            // --- ARRANGE ---
            var loginDto = new UserDataLoginDto { Email = "naoexiste@email.com", Senha = "123" };
            var mockRepo = new Mock<IRepository<Users>>();
            var mockJWT = new Mock<IConfiguration>();

            // Configura o mock para retornar NULO, simulando que o usuário não foi encontrado.
            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync((Users)null);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object);

            // --- ACT ---
            // Usamos Record.ExceptionAsync para capturar a exceção que o método deve lançar.
            var exception = await Record.ExceptionAsync(() => service.LoginUser(loginDto));

            // --- ASSERT ---
            Assert.NotNull(exception); // Afirma que uma exceção FOI lançada.
            Assert.IsType<Exception>(exception); // Afirma que é do tipo esperado.
            Assert.Equal("Credênciais inválidas!", exception.Message); // Afirma que a mensagem está correta.
        }

        // Cenário 2: Teste de falha quando o e-mail existe, mas a senha está errada.
        [Fact]
        public async Task LoginUser_QuandoSenhaIncorreta_DeveLancarExcecao()
        {
            // --- ARRANGE ---
            var senhaCorreta = "SenhaForte123";
            var senhaIncorreta = "senhaErrada";
            var senhaHashCorreta = BCrypt.Net.BCrypt.HashPassword(senhaCorreta);
            var mockJWT = new Mock<IConfiguration>();

            var loginDto = new UserDataLoginDto { Email = "usuario@existente.com", Senha = senhaIncorreta };

            // Cria um usuário falso que seria retornado pelo banco
            var usuarioDoBanco = new Users { Email = loginDto.Email, Senha = senhaHashCorreta };

            var mockRepo = new Mock<IRepository<Users>>();

            // Configura o mock para retornar o usuário, simulando que o e-mail foi encontrado.
            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync(usuarioDoBanco);

            var service = new RegisterAndLoginServ(mockRepo.Object, mockJWT.Object);

            // --- ACT ---
            var exception = await Record.ExceptionAsync(() => service.LoginUser(loginDto));

            // --- ASSERT ---
            Assert.NotNull(exception);
            Assert.Equal("Credênciais inválidas!", exception.Message);
        }

        // Cenário 3: Teste de sucesso (caminho feliz).
        [Fact]
        public async Task LoginUser_ComCredenciaisCorretas_DeveRetornarToken()
        {
            // --- ARRANGE ---
            var senhaCorreta = "SenhaForte123";
            var senhaHashCorreta = BCrypt.Net.BCrypt.HashPassword(senhaCorreta);
            var loginDto = new UserDataLoginDto { Email = "usuario@existente.com", Senha = senhaCorreta };
            var usuarioDoBanco = new Users { Id = Guid.NewGuid(), Email = loginDto.Email, Senha = senhaHashCorreta };

            // 1. Criar a configuração em memória
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:SecretKey", "A9p$qR!sV*zW2&xY#bC4@dE7@fG8hJkL6mN5oP3tUvWzY$B&C*D(F+H)K-M"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // 2. Configurar o mock do repositório
            var mockRepo = new Mock<IRepository<Users>>();
            mockRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Users, bool>>>()))
                    .ReturnsAsync(usuarioDoBanco);

            // 3. Criar a instância do serviço passando a configuração real em memória
            var service = new RegisterAndLoginServ(mockRepo.Object, configuration); // <-- Passe a configuração aqui

            // --- ACT ---
            string token = null;
            var exception = await Record.ExceptionAsync(async () => {
                token = await service.LoginUser(loginDto);
            });

            // --- ASSERT ---
            Assert.Null(exception); // Afirma que NENHUMA exceção foi lançada
            Assert.False(string.IsNullOrEmpty(token)); // Afirma que o token foi gerado
        }
    }
}
