using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.AiConnection;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Interfaces.SnackService;
using NutriTrack_Domains.Tables.UsersTb;
using System.Text.Json;

namespace NutriTrack_Services.SnackService
{
    public class RefeicaoService : IRefeicaoService
    {
        private readonly IRepository<Refeicao> _refeicaoRepository;
        private readonly IRepository<AlimentosConsumido> _alimentosConsumidoRepository;
        private readonly IAiConnectionService _geminiService;

        public RefeicaoService(
            IRepository<Refeicao> refeicaoRepository,
            IRepository<AlimentosConsumido> alimentosConsumidoRepository,
            IAiConnectionService geminiService)
        {
            _refeicaoRepository = refeicaoRepository;
            _alimentosConsumidoRepository = alimentosConsumidoRepository;
            _geminiService = geminiService;
        }

        public async Task<RefeicaoDto> ProcessarECriarRefeicao(Guid usuarioId, string descricaoRefeicao, string nomeRefeicao)
        {
            var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);
            try
            {
                var (tipoRefeicao, descricaoLimpa) = ExtrairTipoRefeicao(descricaoRefeicao);
                var nomeRefeicaoFinal = !string.IsNullOrEmpty(tipoRefeicao)
                    ? tipoRefeicao
                    : (string.IsNullOrEmpty(nomeRefeicao) ? "Refeição" : nomeRefeicao);

                var prompt = MontarPromptAnaliseNutricional(descricaoLimpa, nomeRefeicaoFinal);
                var respostaIA = await _geminiService.AnalisarRefeicao(prompt);
                var erroValidacao = VerificarErroValidacao(respostaIA);
                if (erroValidacao != null)
                {
                    throw new ArgumentException(erroValidacao);
                }

                var alimentosAnalisados = ParsearRespostaGemini(respostaIA);

                if (alimentosAnalisados == null || !alimentosAnalisados.Any())
                {
                    throw new Exception("Não foi possível analisar os alimentos da refeição.");
                }

                var refeicao = new Refeicao
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    NomeRef = nomeRefeicaoFinal,
                    Data = DateOnly.FromDateTime(dataHoraBrasilia)
                };

                await _refeicaoRepository.AddAsync(refeicao);

                foreach (var alimento in alimentosAnalisados)
                {
                    var alimentoConsumido = new AlimentosConsumido
                    {
                        Id = Guid.NewGuid(),
                        RefeicaoId = refeicao.Id,
                        Descricao = alimento.Descricao,
                        Quantidade = alimento.Quantidade,
                        Unidade = alimento.Unidade,
                        Calorias = alimento.Calorias,
                        Proteinas = alimento.Proteinas,
                        Carboidratos = alimento.Carboidratos,
                        Gorduras = alimento.Gorduras
                    };

                    await _alimentosConsumidoRepository.AddAsync(alimentoConsumido);
                }

                var refeicaoCompleta = await ObterRefeicaoPorId(refeicao.Id);
                return refeicaoCompleta;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Limite de tokens ultrapassado ou falha na comunicação com o serviço de IA. Por favor, tente mais tarde!");
            }
        }

        private (string tipoRefeicao, string descricaoLimpa) ExtrairTipoRefeicao(string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return (null, descricao);

            var tiposRefeicao = new[]
            {
                "café da manhã", "café", "breakfast",
                "almoço", "almoco", "lunch",
                "jantar", "janta", "dinner",
                "lanche da manhã", "lanche da tarde", "lanche",
                "ceia", "snack",
                "pré-treino", "pré", "pré working", "pré treino", "pré-working", "pre treino", "pre treino", "pre working",
                "pós-treino", "pós", "pós working", "pós treino", "pós-working", "pos treino", "pos treino", "pos working",
            };

            var descricaoLower = descricao.ToLower().Trim();

            foreach (var tipo in tiposRefeicao)
            {
                if (descricaoLower.StartsWith(tipo + ",") ||
                    descricaoLower.StartsWith(tipo + ":") ||
                    descricaoLower.StartsWith(tipo + " "))
                {
                    var descricaoSemTipo = descricao.Substring(tipo.Length).TrimStart(',', ':', ' ');
                    var tipoNormalizado = CapitalizarPrimeiraLetra(tipo);

                    return (tipoNormalizado, descricaoSemTipo);
                }

                if (descricaoLower == tipo)
                {
                    return (CapitalizarPrimeiraLetra(tipo), string.Empty);
                }
            }

            return (null, descricao);
        }

        private string CapitalizarPrimeiraLetra(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            return char.ToUpper(texto[0]) + texto.Substring(1).ToLower();
        }

        private string VerificarErroValidacao(string respostaIA)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(respostaIA))
                {
                    var root = document.RootElement;

                    if (root.TryGetProperty("erro", out var erroElement))
                    {
                        if (root.TryGetProperty("mensagem", out var mensagemElement))
                        {
                            return mensagemElement.GetString();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public async Task<List<RefeicaoDto>> ObterRefeicoesDoUsuario(Guid usuarioId, DateOnly? data = null)
        {
            try
            {
                var refeicoes = await _refeicaoRepository.GetAllAsync(r => r.UsuarioId == usuarioId);

                if (data.HasValue)
                {
                    refeicoes = refeicoes.Where(r => r.Data == data.Value).ToList();
                }

                var refeicoesDto = new List<RefeicaoDto>();

                foreach (var refeicao in refeicoes.OrderByDescending(r => r.Data).ThenBy(r => r.Id))
                {
                    var dto = await MapearParaDto(refeicao);
                    refeicoesDto.Add(dto);
                }

                return refeicoesDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar refeições: {ex.Message}");
            }
        }

        public async Task<RefeicaoDto> ObterRefeicaoPorId(Guid refeicaoId)
        {
            try
            {
                var refeicao = await _refeicaoRepository.GetByIdAsync(refeicaoId);

                if (refeicao == null)
                {
                    throw new Exception("Refeição não encontrada");
                }

                return await MapearParaDto(refeicao);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar refeição: {ex.Message}");
            }
        }

        private string MontarPromptAnaliseNutricional(string descricaoRefeicao, string tipoRefeicao)
        {
            return $@"Você é um assistente de análise nutricional. Analise a seguinte descrição de refeição.
                    TIPO DE REFEIÇÃO: {tipoRefeicao}
                    DESCRIÇÃO DO USUÁRIO: {descricaoRefeicao}

                    REGRAS DE VALIDAÇÃO (execute nesta ordem):
                    1. VERIFICAR SE HÁ ALIMENTOS: Se a descrição não contiver nenhum alimento identificável, retorne:
                       {{
                         ""erro"": ""MENSAGEM_INVALIDA"",
                         ""mensagem"": ""Não foi possível identificar nenhum alimento na descrição. Por favor, informe o que você comeu.""
                       }}

                    2. VERIFICAR QUANTIDADES: Se houver alimentos identificáveis MAS nenhuma quantidade especificada (porções, gramas, unidades, etc.), retorne:
                       {{
                         ""erro"": ""QUANTIDADE_NAO_INFORMADA"",
                         ""mensagem"": ""Por favor, informe a quantidade de cada alimento que você consumiu (exemplo: 200g de arroz, 1 filé de frango, 2 fatias de pão).""
                       }}

                    3. SE TUDO ESTIVER CORRETO: Retorne a análise nutricional no formato:
                       {{
                         ""alimentos"": [
                           {{
                             ""descricao"": ""nome do alimento"",
                             ""quantidade"": 0.0,
                             ""unidade"": ""g/ml/unidade/fatia/colher"",
                             ""calorias"": 0.0,
                             ""proteinas"": 0.0,
                             ""carboidratos"": 0.0,
                             ""gorduras"": 0.0
                           }}
                         ]
                       }}

                    IMPORTANTE:
                    - Retorne APENAS JSON válido, sem markdown, sem ```json
                    - Seja preciso nos valores nutricionais usando tabelas TACO/USDA
                    - Se quantidade for aproximada (""um pouco"", ""bastante""), peça quantidade específica
                    - Aceite quantidades em: gramas, ml, unidades, fatias, colheres, xícaras, etc.";
        }

        private List<AlimentoAnalisadoDto> ParsearRespostaGemini(string respostaJson)
        {
            try
            {
                var jsonLimpo = respostaJson.Replace("```json", "").Replace("```", "").Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var resposta = JsonSerializer.Deserialize<GeminiParseResponse>(jsonLimpo, options);

                if (resposta?.Alimentos == null || !resposta.Alimentos.Any())
                {
                    throw new Exception("A IA não retornou alimentos válidos");
                }

                return resposta.Alimentos.Select(a => new AlimentoAnalisadoDto
                {
                    Descricao = a.Descricao,
                    Quantidade = a.Quantidade,
                    Unidade = a.Unidade ?? "unidade",
                    Calorias = a.Calorias,
                    Proteinas = a.Proteinas,
                    Carboidratos = a.Carboidratos,
                    Gorduras = a.Gorduras
                }).ToList();
            }
            catch (JsonException ex)
            {
                throw new Exception($"Erro ao interpretar resposta da IA: {ex.Message}");
            }
        }

        private async Task<RefeicaoDto> MapearParaDto(Refeicao refeicao)
        {
            var alimentos = await _alimentosConsumidoRepository.GetAllAsync(a => a.RefeicaoId == refeicao.Id);

            var alimentosDto = alimentos.Select(a => new AlimentoConsumidoDto
            {
                Id = a.Id,
                Descricao = a.Descricao,
                Quantidade = a.Quantidade,
                Unidade = a.Unidade,
                Calorias = a.Calorias,
                Proteinas = a.Proteinas,
                Carboidratos = a.Carboidratos,
                Gorduras = a.Gorduras
            }).ToList();

            return new RefeicaoDto
            {
                Id = refeicao.Id,
                NomeRef = refeicao.NomeRef,
                Data = refeicao.Data,
                Alimentos = alimentosDto,
                TotalCalorias = alimentosDto.Sum(a => a.Calorias),
                TotalProteinas = alimentosDto.Sum(a => a.Proteinas),
                TotalCarboidratos = alimentosDto.Sum(a => a.Carboidratos),
                TotalGorduras = alimentosDto.Sum(a => a.Gorduras)
            };
        }

        public async Task<RefeicaoDto> AtualizarRefeicao(Guid refeicaoId, Guid usuarioId, string descricaoRefeicao, string nomeRefeicao)
        {
            try
            {
                var refeicaoExistente = await _refeicaoRepository.GetByIdAsync(refeicaoId);

                if (refeicaoExistente == null)
                {
                    throw new Exception("Refeição não encontrada");
                }

                if (refeicaoExistente.UsuarioId != usuarioId)
                {
                    throw new UnauthorizedAccessException("Você não tem permissão para editar esta refeição");
                }

                var alimentosAntigos = await _alimentosConsumidoRepository.GetAllAsync(a => a.RefeicaoId == refeicaoId);
                foreach (var alimento in alimentosAntigos)
                {
                    await _alimentosConsumidoRepository.DeleteAsync(alimento);
                }

                var (tipoRefeicao, descricaoLimpa) = ExtrairTipoRefeicao(descricaoRefeicao);

                var nomeRefeicaoFinal = !string.IsNullOrEmpty(tipoRefeicao)
                    ? tipoRefeicao
                    : (string.IsNullOrEmpty(nomeRefeicao) ? refeicaoExistente.NomeRef : nomeRefeicao);

                var prompt = MontarPromptAnaliseNutricional(descricaoLimpa, nomeRefeicaoFinal);
                var respostaIA = await _geminiService.AnalisarRefeicao(prompt);

                var erroValidacao = VerificarErroValidacao(respostaIA);
                if (erroValidacao != null)
                {
                    throw new ArgumentException(erroValidacao);
                }

                var alimentosAnalisados = ParsearRespostaGemini(respostaIA);

                if (alimentosAnalisados == null || !alimentosAnalisados.Any())
                {
                    throw new Exception("Não foi possível analisar os alimentos da refeição.");
                }

                refeicaoExistente.NomeRef = nomeRefeicaoFinal;
                await _refeicaoRepository.UpdateAsync(refeicaoExistente);

                foreach (var alimento in alimentosAnalisados)
                {
                    var alimentoConsumido = new AlimentosConsumido
                    {
                        Id = Guid.NewGuid(),
                        RefeicaoId = refeicaoExistente.Id,
                        Descricao = alimento.Descricao,
                        Quantidade = alimento.Quantidade,
                        Unidade = alimento.Unidade,
                        Calorias = alimento.Calorias,
                        Proteinas = alimento.Proteinas,
                        Carboidratos = alimento.Carboidratos,
                        Gorduras = alimento.Gorduras
                    };

                    await _alimentosConsumidoRepository.AddAsync(alimentoConsumido);
                }

                var refeicaoAtualizada = await ObterRefeicaoPorId(refeicaoExistente.Id);
                return refeicaoAtualizada;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar refeição: {ex.Message}");
            }
        }

        public async Task<bool> ExcluirRefeicao(Guid refeicaoId, Guid usuarioId)
        {
            try
            {
                var refeicao = await _refeicaoRepository.GetByIdAsync(refeicaoId);

                if (refeicao == null)
                {
                    throw new Exception("Refeição não encontrada");
                }

                if (refeicao.UsuarioId != usuarioId)
                {
                    throw new UnauthorizedAccessException("Você não tem permissão para excluir esta refeição");
                }

                var alimentos = await _alimentosConsumidoRepository.GetAllAsync(a => a.RefeicaoId == refeicaoId);
                foreach (var alimento in alimentos)
                {
                    await _alimentosConsumidoRepository.DeleteAsync(alimento);
                }

                await _refeicaoRepository.DeleteAsync(refeicao);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao excluir refeição: {ex.Message}");
            }
        }

        public async Task<RefeicaoDto> AtualizarNomeRefeicao(Guid refeicaoId, Guid usuarioId, string novoNome)
        {
            try
            {
                var refeicaoExistente = await _refeicaoRepository.GetByIdAsync(refeicaoId);

                if (refeicaoExistente == null)
                {
                    throw new Exception("Refeição não encontrada");
                }

                if (refeicaoExistente.UsuarioId != usuarioId)
                {
                    throw new UnauthorizedAccessException("Você não tem permissão para editar esta refeição");
                }

                refeicaoExistente.NomeRef = novoNome;
                await _refeicaoRepository.UpdateAsync(refeicaoExistente);

                var refeicaoAtualizada = await ObterRefeicaoPorId(refeicaoExistente.Id);
                return refeicaoAtualizada;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar nome da refeição: {ex.Message}");
            }
        }

        private class GeminiParseResponse
        {
            public List<AlimentoJson> Alimentos { get; set; }
        }

        private class AlimentoJson
        {
            public string Descricao { get; set; }
            public double Quantidade { get; set; }
            public string Unidade { get; set; }
            public double Calorias { get; set; }
            public double Proteinas { get; set; }
            public double Carboidratos { get; set; }
            public double Gorduras { get; set; }
        }
    }
}
