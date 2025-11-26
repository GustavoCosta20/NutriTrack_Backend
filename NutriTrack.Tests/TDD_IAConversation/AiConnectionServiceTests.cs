using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NutriTrack_Services.IaConexionService;
using Xunit;

namespace NutriTrack_Tests.TDD_IAConversation
{
    public class AiConnectionServiceTests
    {
        [Fact]
        public async Task AnalisarRefeicao_DeveRetornarTextoQuandoRespostaValida()
        {
            // Arrange
            var prompt = "Teste";
            var respostaJson = """
        {
            "candidates": [
                {
                    "content": {
                        "parts": [
                            { "text": "Resposta da IA" }
                        ]
                    }
                }
            ]
        }
        """;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(respostaJson)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Gemini:ApiKey"]).Returns("fake-api-key");

            var service = new AiConnectionService(httpClient, configMock.Object, null, null);

            // Act
            var resultado = await service.AnalisarRefeicao(prompt);

            // Assert
            Assert.Equal("Resposta da IA", resultado);
        }

        [Fact]
        public async Task AnalisarRefeicao_DeveLancarExcecaoQuandoStatusNaoSucesso()
        {
            // Arrange
            var prompt = "Teste";
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Erro")
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Gemini:ApiKey"]).Returns("fake-api-key");

            var service = new AiConnectionService(httpClient, configMock.Object, null, null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.AnalisarRefeicao(prompt));
            Assert.Contains("Erro ao chamar API Gemini", ex.Message);
        }

        [Fact]
        public async Task AnalisarRefeicao_DeveLancarExcecaoQuandoRespostaVazia()
        {
            // Arrange
            var prompt = "Teste";
            var respostaJson = """
        {
            "candidates": [
                {
                    "content": {
                        "parts": [
                            { "text": "" }
                        ]
                    }
                }
            ]
        }
        """;

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(respostaJson)
                });

            var httpClient = new HttpClient(handlerMock.Object);

            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Gemini:ApiKey"]).Returns("fake-api-key");

            var service = new AiConnectionService(httpClient, configMock.Object, null, null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.AnalisarRefeicao(prompt));
            Assert.Contains("A API Gemini não retornou uma resposta válida", ex.Message);
        }
    }
}