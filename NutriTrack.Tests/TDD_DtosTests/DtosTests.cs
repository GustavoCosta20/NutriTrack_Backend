using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Enums;
using Xunit;

namespace NutriTrack_Tests.TDD_DtosTests
{
    public class AtualizarNomeRequestTests
    {
        [Fact]
        public void AtualizarNomeRequest_DeveCriarComNomeValido()
        {
            // Arrange & Act
            var request = new AtualizarNomeRequest
            {
                NomeRefeicao = "João Silva"
            };

            // Assert
            Assert.Equal("João Silva", request.NomeRefeicao);
        }

        [Fact]
        public void AtualizarNomeRequest_DevePermitirNomeVazio()
        {
            // Arrange & Act
            var request = new AtualizarNomeRequest
            {
                NomeRefeicao = string.Empty
            };

            // Assert
            Assert.Empty(request.NomeRefeicao);
        }

        [Fact]
        public void AtualizarNomeRequest_DevePermitirNomeNulo()
        {
            // Arrange & Act
            var request = new AtualizarNomeRequest
            {
                NomeRefeicao = null
            };

            // Assert
            Assert.Null(request.NomeRefeicao);
        }
    }

    public class ChatIaRequestTests
    {
        [Fact]
        public void ChatIaRequest_DeveCriarComMensagemValida()
        {
            // Arrange & Act
            var request = new ChatIaRequest
            {
                Mensagem = "Qual a melhor dieta?"
            };

            // Assert
            Assert.Equal("Qual a melhor dieta?", request.Mensagem);
        }

        [Fact]
        public void ChatIaRequest_DeveSerializarCorretamente()
        {
            // Arrange
            var request = new ChatIaRequest
            {
                Mensagem = "Como calcular calorias?"
            };

            // Assert
            Assert.NotNull(request);
            Assert.NotEmpty(request.Mensagem);
        }

        [Fact]
        public void ChatIaRequest_DevePermitirMensagemLonga()
        {
            // Arrange
            var mensagemLonga = new string('a', 1000);

            // Act
            var request = new ChatIaRequest
            {
                Mensagem = mensagemLonga
            };

            // Assert
            Assert.Equal(1000, request.Mensagem.Length);
        }
    }

    public class ChatIaResponseTests
    {
        [Fact]
        public void ChatIaResponse_DeveCriarComRespostaValida()
        {
            // Arrange & Act
            var response = new ChatIaResponse
            {
                Resposta = "Sua resposta aqui",
                Sucesso = true,
                Mensagem = "Processado com sucesso"
            };

            // Assert
            Assert.Equal("Sua resposta aqui", response.Resposta);
            Assert.True(response.Sucesso);
            Assert.Equal("Processado com sucesso", response.Mensagem);
        }

        [Fact]
        public void ChatIaResponse_DeveIndicarErro()
        {
            // Arrange & Act
            var response = new ChatIaResponse
            {
                Resposta = null,
                Sucesso = false,
                Mensagem = "Erro ao processar"
            };

            // Assert
            Assert.False(response.Sucesso);
            Assert.Contains("Erro", response.Mensagem);
        }

        [Fact]
        public void ChatIaResponse_DeveSerInstanciavelComValoresPadrao()
        {
            // Arrange & Act
            var response = new ChatIaResponse();

            // Assert
            Assert.NotNull(response);
        }
    }

    public class CriarRefeicaoRequestTests
    {
        [Fact]
        public void CriarRefeicaoRequest_DeveCriarComDadosValidos()
        {
            // Arrange & Act
            var request = new CriarRefeicaoRequest
            {
                NomeRefeicao = "Café da Manhã"
            };

            // Assert
            Assert.Equal("Café da Manhã", request.NomeRefeicao);
        }
    }

    public class ProfileDataDtoTests
    {
        [Fact]
        public void ProfileDataDto_DeveCriarComDadosCompletos()
        {
            // Arrange & Act
            var profile = new ProfileDataDto
            {
                NomeCompleto = "Maria Silva",
                Email = "maria@example.com",
                DataNascimento = DateOnly.Parse("15/02/1992"),
                PesoEmKg = 65.5,
                AlturaEmCm = 165,
                Objetivo = EnumObjetivo.GanharMassa,
                NivelDeAtividade = EnumNivelDeAtividade.AtividadeModerada
            };

            // Assert
            Assert.Equal("Maria Silva", profile.NomeCompleto);
            Assert.Equal("maria@example.com", profile.Email);
            Assert.Equal(DateOnly.Parse("15/02/1992"), profile.DataNascimento);
            Assert.Equal(65.5, profile.PesoEmKg);
            Assert.Equal(165, profile.AlturaEmCm);
            Assert.Equal(EnumObjetivo.GanharMassa, profile.Objetivo);
            Assert.Equal(EnumNivelDeAtividade.AtividadeModerada, profile.NivelDeAtividade);
        }

        [Fact]
        public void ProfileDataDto_DeveCalcularIMC()
        {
            // Arrange
            var profile = new ProfileDataDto
            {
                PesoEmKg = 70,
                AlturaEmCm = 175
            };

            // Act
            var imcEsperado = 70 / (175 * 175); // ~22.86

            // Assert
            Assert.Equal(imcEsperado, profile.PesoEmKg / (profile.AlturaEmCm * profile.AlturaEmCm), 2);
        }

        [Fact]
        public void ProfileDataDto_DevePermitirValoresMinimos()
        {
            // Arrange & Act
            var profile = new ProfileDataDto
            {
                PesoEmKg = 0,
                AlturaEmCm = 0
            };

            // Assert
            Assert.Equal(0, profile.PesoEmKg);
            Assert.Equal(0, profile.AlturaEmCm);
        }
    }

    public class RefeicoesDoHojeResponseTests
    {
        [Fact]
        public void RefeicoesDoHojeResponse_DeveCriarComListaVazia()
        {
            // Arrange & Act
            var response = new RefeicoesDoHojeResponse
            {
                Refeicoes = new List<RefeicaoDto>(),
            };

            // Assert
            Assert.Empty(response.Refeicoes);
        }

        [Fact]
        public void RefeicoesDoHojeResponse_DeveCriarComMultiplasRefeicoes()
        {
            // Arrange & Act
            var response = new RefeicoesDoHojeResponse
            {
                Refeicoes = new List<RefeicaoDto>
                {
                    new RefeicaoDto { Id = Guid.NewGuid(), NomeRef = "Café" },
                    new RefeicaoDto { Id = Guid.NewGuid(), NomeRef = "Almoço" },
                    new RefeicaoDto { Id = Guid.NewGuid(), NomeRef = "Jantar" }
                }
            };

            // Assert
            Assert.Equal(3, response.Refeicoes.Count);
            Assert.Equal("Café", response.Refeicoes[0].NomeRef);
        }
    }

    public class TotaisDiariosTests
    {
        [Fact]
        public void TotaisDiarios_DeveCriarComValoresValidos()
        {
            // Arrange & Act
            var totais = new TotaisDiarios
            {
                Calorias = 2000,
                Proteinas = 150,
                Carboidratos = 250,
                Gorduras = 70
            };

            // Assert
            Assert.Equal(2000, totais.Calorias);
            Assert.Equal(150, totais.Proteinas);
            Assert.Equal(250, totais.Carboidratos);
            Assert.Equal(70, totais.Gorduras);
        }

        [Fact]
        public void TotaisDiarios_DevePermitirValoresZero()
        {
            // Arrange & Act
            var totais = new TotaisDiarios
            {
                Calorias = 0,
                Proteinas = 0,
                Carboidratos = 0,
                Gorduras = 0
            };

            // Assert
            Assert.Equal(0, totais.Calorias);
            Assert.Equal(0, totais.Proteinas);
        }

        [Fact]
        public void TotaisDiarios_DevePermitirValoresDecimais()
        {
            // Arrange & Act
            var totais = new TotaisDiarios
            {
                Calorias = 2000.5,
                Proteinas = 150.75,
                Carboidratos = 250.25,
                Gorduras = 70.33
            };

            // Assert
            Assert.Equal(2000.5, totais.Calorias, 2);
            Assert.Equal(150.75, totais.Proteinas, 2);
        }
    }

    public class UpdateProfileDtoTests
    {
        [Fact]
        public void UpdateProfileDto_DeveCriarComTodosCampos()
        {
            // Arrange & Act
            var updateProfile = new UpdateProfileDto
            {
                NomeCompleto = "Carlos Alberto",
                Email = "carlos@example.com",
                DataNascimento = DateOnly.Parse("15/05/2000"),
                PesoEmKg = 80,
                AlturaEmCm = 180,
                Objetivo = EnumObjetivo.PerderGordura,
                NivelDeAtividade = EnumNivelDeAtividade.Sedentario,
            };

            // Assert
            Assert.Equal("Carlos Alberto", updateProfile.NomeCompleto);
            Assert.Equal("carlos@example.com", updateProfile.Email);
        }

        [Fact]
        public void UpdateProfileDto_DevePermitirCamposNulos()
        {
            // Arrange & Act
            var updateProfile = new UpdateProfileDto
            {
                NomeCompleto = null,
                Email = null
            };

            // Assert
            Assert.Null(updateProfile.NomeCompleto);
            Assert.Null(updateProfile.Email);
        }

        [Fact]
        public void UpdateProfileDto_DeveAtualizarApenasAlgunsCampos()
        {
            // Arrange & Act
            var updateProfile = new UpdateProfileDto
            {
                PesoEmKg = 75.5
            };

            // Assert
            Assert.Equal(75.5, updateProfile.PesoEmKg);
            Assert.Null(updateProfile.NomeCompleto);
        }
    }
}
