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
            try
            {
                var prompt = MontarPromptAnaliseNutricional(descricaoRefeicao);
                var respostaIA = await _geminiService.AnalisarRefeicao(prompt);
                var alimentosAnalisados = ParsearRespostaGemini(respostaIA);

                if (alimentosAnalisados == null || !alimentosAnalisados.Any())
                {
                    throw new Exception("Não foi possível analisar os alimentos da refeição.");
                }

                var refeicao = new Refeicao
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    NomeRef = string.IsNullOrEmpty(nomeRefeicao) ? "Refeição" : nomeRefeicao,
                    Data = DateOnly.FromDateTime(DateTime.UtcNow)
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
            catch (Exception ex)
            {
                throw new Exception($"Erro ao processar refeição: {ex.Message}");
            }
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

                foreach (var refeicao in refeicoes.OrderByDescending(r => r.Data))
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

        private string MontarPromptAnaliseNutricional(string descricaoRefeicao)
        {
            return $@"Analise a seguinte descrição de refeição e retorne APENAS um JSON válido (sem markdown, sem ```json) com as informações nutricionais detalhadas de cada alimento.

                    Descrição: {descricaoRefeicao}

                    IMPORTANTE:
                    - Seja preciso nas quantidades e valores nutricionais
                    - Use valores realistas baseados em tabelas nutricionais
                    - Se a quantidade não for especificada, use porções padrão
                    - Retorne APENAS o JSON, sem texto adicional

                    Formato de resposta (APENAS JSON):
                    {{
                      ""alimentos"": [
                        {{
                          ""descricao"": ""nome do alimento"",
                          ""quantidade"": 0.0,
                          ""unidade"": ""g/ml/unidade/fatia"",
                          ""calorias"": 0.0,
                          ""proteinas"": 0.0,
                          ""carboidratos"": 0.0,
                          ""gorduras"": 0.0
                        }}
                      ]
                    }}";
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
