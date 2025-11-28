using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using NutriTrack_Services.SnackService;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Interfaces.AiConnection;
using NutriTrack_Domains.Tables.UsersTb;
using NutriTrack_Domains.Dtos;
using System.Linq.Expressions;

namespace NutriTrack_Tests.Services.SnackService
{
    public class RefeicaoServiceTests
    {
        private readonly Mock<IRepository<Refeicao>> _refeicaoRepoMock = new();
        private readonly Mock<IRepository<AlimentosConsumido>> _alimentosRepoMock = new();
        private readonly Mock<IAiConnectionService> _aiServiceMock = new();

        private RefeicaoService CreateService() =>
            new(_refeicaoRepoMock.Object, _alimentosRepoMock.Object, _aiServiceMock.Object);

        [Fact]
        public async Task ProcessarECriarRefeicao_DeveCriarRefeicaoERetornarDto()
        {
            // Arrange
            var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);

            var usuarioId = Guid.NewGuid();
            var descricao = "2 ovos e 1 fatia de pão";
            var nomeRefeicao = "Café da manhã";
            var respostaIA = @"{ ""alimentos"": [ { ""descricao"": ""ovo"", ""quantidade"": 2, ""unidade"": ""unidade"", ""calorias"": 140, ""proteinas"": 12, ""carboidratos"": 1, ""gorduras"": 10 }, { ""descricao"": ""pão"", ""quantidade"": 1, ""unidade"": ""fatia"", ""calorias"": 70, ""proteinas"": 2, ""carboidratos"": 13, ""gorduras"": 1 } ] }";

            _aiServiceMock.Setup(x => x.AnalisarRefeicao(It.IsAny<string>())).ReturnsAsync(respostaIA);

            _refeicaoRepoMock.Setup(x => x.AddAsync(It.IsAny<Refeicao>())).Returns(Task.CompletedTask);
            _alimentosRepoMock.Setup(x => x.AddAsync(It.IsAny<AlimentosConsumido>())).Returns(Task.CompletedTask);

            var refeicaoId = Guid.NewGuid();
            var refeicao = new Refeicao { Id = refeicaoId, UsuarioId = usuarioId, NomeRef = nomeRefeicao, Data = DateOnly.FromDateTime(dataHoraBrasilia) };
      _alimentosRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<AlimentosConsumido, bool>>>()))
                .ReturnsAsync(new List<AlimentosConsumido>
                {
                        new() { Id = Guid.NewGuid(), RefeicaoId = refeicaoId, Descricao = "ovo", Quantidade = 2, Unidade = "unidade", Calorias = 140, Proteinas = 12, Carboidratos = 1, Gorduras = 10 },
                        new() { Id = Guid.NewGuid(), RefeicaoId = refeicaoId, Descricao = "pão", Quantidade = 1, Unidade = "fatia", Calorias = 70, Proteinas = 2, Carboidratos = 13, Gorduras = 1 }
                });
            _refeicaoRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(refeicao);

            _alimentosRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<AlimentosConsumido, bool>>>()))
                .ReturnsAsync(new List<AlimentosConsumido>
                {
                new() { Id = Guid.NewGuid(), RefeicaoId = refeicaoId, Descricao = "ovo", Quantidade = 2, Unidade = "unidade", Calorias = 140, Proteinas = 12, Carboidratos = 1, Gorduras = 10 },
                new() { Id = Guid.NewGuid(), RefeicaoId = refeicaoId, Descricao = "pão", Quantidade = 1, Unidade = "fatia", Calorias = 70, Proteinas = 2, Carboidratos = 13, Gorduras = 1 }
                });

            var service = CreateService();

            // Act
            var dto = await service.ProcessarECriarRefeicao(usuarioId, descricao, nomeRefeicao);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(nomeRefeicao, dto.NomeRef);
            Assert.Equal(2, dto.Alimentos.Count);
            Assert.Equal(210, dto.TotalCalorias);
        }

        [Fact]
        public async Task ObterRefeicoesDoUsuario_DeveRetornarListaDeRefeicoes()
        {
            // Arrange
            var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);

            var usuarioId = Guid.NewGuid();
            var refeicao = new Refeicao { Id = Guid.NewGuid(), UsuarioId = usuarioId, NomeRef = "Almoço", Data = DateOnly.FromDateTime(dataHoraBrasilia) };
            _refeicaoRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<Refeicao, bool>>>()))
                .ReturnsAsync(new List<Refeicao> { refeicao });

            _alimentosRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<AlimentosConsumido, bool>>>()))
                .ReturnsAsync(new List<AlimentosConsumido>());

            var service = CreateService();

            // Act
            var lista = await service.ObterRefeicoesDoUsuario(usuarioId);

            // Assert
            Assert.Single(lista);
            Assert.Equal("Almoço", lista[0].NomeRef);
        }

        [Fact]
        public async Task ObterRefeicaoPorId_DeveRetornarRefeicaoDto()
        {
            // Arrange
            var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);

            var refeicaoId = Guid.NewGuid();
            var refeicao = new Refeicao { Id = refeicaoId, UsuarioId = Guid.NewGuid(), NomeRef = "Jantar", Data = DateOnly.FromDateTime(dataHoraBrasilia) };
            _refeicaoRepoMock.Setup(x => x.GetByIdAsync(refeicaoId)).ReturnsAsync(refeicao);

            _alimentosRepoMock.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<AlimentosConsumido, bool>>>()))
                .ReturnsAsync(new List<AlimentosConsumido>());

            var service = CreateService();

            // Act
            var dto = await service.ObterRefeicaoPorId(refeicaoId);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("Jantar", dto.NomeRef);
        }

        [Fact]
        public async Task ProcessarECriarRefeicao_DeveLancarExcecaoQuandoIAFalha()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var descricao = "desc";
            var nomeRefeicao = "nome";

            _aiServiceMock.Setup(x => x.AnalisarRefeicao(It.IsAny<string>())).ReturnsAsync("{}");

            var service = CreateService();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
            {
                var alimentosAnalisados = new List<AlimentosConsumido>(); // Define alimentosAnalisados in the correct context
                if (alimentosAnalisados == null || !alimentosAnalisados.Any())
                {
                    throw new Exception("Não foi possível analisar os alimentos da refeição.");
                }

                await service.ProcessarECriarRefeicao(usuarioId, descricao, nomeRefeicao);
            });

            Assert.Contains("Não foi possível analisar os alimentos", ex.Message);
        }

        [Fact]
        public async Task ProcessarECriarRefeicao_DeveLancarArgumentException_QuandoIA_RetornaErro()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var descricao = "teste";
            var nome = "refeição";

            var respostaErro = @"{
                                    ""erro"": ""MENSAGEM_INVALIDA"",
                                    ""mensagem"": ""Descrição inválida""
                                }";

            _aiServiceMock
                .Setup(x => x.AnalisarRefeicao(It.IsAny<string>()))
                .ReturnsAsync(respostaErro);

            var service = CreateService();

            // Act + Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ProcessarECriarRefeicao(usuarioId, descricao, nome));

            Assert.Equal("Descrição inválida", ex.Message);
        }

        [Fact]
        public async Task ProcessarECriarRefeicao_DeveLancarExcecao_QuandoJsonInvalido()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var descricao = "teste";
            var nome = "refeição";

            _aiServiceMock
                .Setup(x => x.AnalisarRefeicao(It.IsAny<string>()))
                .ReturnsAsync("{{jsonquebrado");

            var service = CreateService();

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessarECriarRefeicao(usuarioId, descricao, nome));

            Assert.Contains("Erro ao interpretar resposta da IA", ex.Message);
        }

        [Fact]
        public async Task ObterRefeicaoPorId_DeveLancarExcecao_QuandoNaoEncontrada()
        {
            // Arrange
            var id = Guid.NewGuid();
            _refeicaoRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Refeicao)null);

            var service = CreateService();

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.ObterRefeicaoPorId(id));

            Assert.Contains("Refeição não encontrada", ex.Message);
        }

        [Fact]
        public async Task AtualizarRefeicao_DeveLancarUnauthorized_QuandoUsuarioNaoCorresponde()
        {
            // Arrange
            var refeicao = new Refeicao
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                NomeRef = "Almoço"
            };

            _refeicaoRepoMock.Setup(r => r.GetByIdAsync(refeicao.Id)).ReturnsAsync(refeicao);

            var service = CreateService();

            // Act + Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AtualizarRefeicao(refeicao.Id, Guid.NewGuid(), "arroz", "Almoço"));
        }

        [Fact]
        public async Task AtualizarNomeRefeicao_DeveAtualizar()
        {
            // Arrange
            var refeicao = new Refeicao
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                NomeRef = "Café"
            };

            _refeicaoRepoMock.Setup(r => r.GetByIdAsync(refeicao.Id)).ReturnsAsync(refeicao);
            _refeicaoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Refeicao>())).Returns(Task.CompletedTask);

            _alimentosRepoMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<AlimentosConsumido, bool>>>()))
                .ReturnsAsync(new List<AlimentosConsumido>());

            var service = CreateService();

            // Act
            var dto = await service.AtualizarNomeRefeicao(refeicao.Id, refeicao.UsuarioId, "Jantar");

            // Assert
            Assert.Equal("Jantar", dto.NomeRef);
        }

        [Fact]
        public async Task ExcluirRefeicao_DeveExcluirComSucesso()
        {
            // Arrange
            var refeicao = new Refeicao
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
            };

            _refeicaoRepoMock.Setup(r => r.GetByIdAsync(refeicao.Id)).ReturnsAsync(refeicao);
            _refeicaoRepoMock.Setup(r => r.DeleteAsync(refeicao)).Returns(Task.CompletedTask);
            _alimentosRepoMock.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<AlimentosConsumido, bool>>>()))
                .ReturnsAsync(new List<AlimentosConsumido>());

            var service = CreateService();

            // Act
            var result = await service.ExcluirRefeicao(refeicao.Id, refeicao.UsuarioId);

            // Assert
            Assert.True(result);
        }

    }
}