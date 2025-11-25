using Microsoft.Extensions.Configuration;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.AiConnection;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Tables.UsersTb;
using System.Net.Http.Json;
using System.Text.Json;

namespace NutriTrack_Services.IaConexionService
{
    public class AiConnectionService : IAiConnectionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly IRepository<Refeicao> _refeicaoRepository;
        private readonly IRepository<AlimentosConsumido> _alimentosConsumidosRepository;

        public AiConnectionService(HttpClient httpClient,
                                   IConfiguration configuration,
                                   IRepository<Refeicao> refeicaoRepository,
                                   IRepository<AlimentosConsumido> alimentosConsumidosRepository)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key não configurada no appsettings.json");
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/" + $"gemini-2.5-flash-preview-09-2025:generateContent?key={_apiKey}";
            _refeicaoRepository = refeicaoRepository;
            _alimentosConsumidosRepository = alimentosConsumidosRepository;
        }

        public async Task<string> AnalisarRefeicao(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        topK = 1,
                        topP = 1,
                        maxOutputTokens = 2048,
                        candidateCount = 1
                    },
                    safetySettings = new[]
                    {
                        new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                        new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                        new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                        new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao chamar API Gemini: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                var textoResposta = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(textoResposta))
                {
                    throw new Exception("A API Gemini não retornou uma resposta válida.");
                }

                return textoResposta;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conexão com a API Gemini: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Erro ao processar resposta da API Gemini: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao analisar refeição com Gemini: {ex.Message}");
            }
        }

        public async Task<string> ConversarSobreNutricao(string mensagemUsuario, Users perfil)
        {
            try
            {
                var timeZoneBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var dataHoraBrasilia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasilia);
                var hoje = DateOnly.FromDateTime(dataHoraBrasilia);
                var refeicoesDia = await _refeicaoRepository.GetAllAsync(r => r.UsuarioId == perfil.Id && r.Data == hoje);
                var alimentosConsumidos = await _alimentosConsumidosRepository.GetAllAsync(a => refeicoesDia.Select(r => r.Id).Contains(a.RefeicaoId));
                var prompt = $@"
                                Você é um assistente nutricional especializado em orientar o usuário sobre alimentação saudável, horários das refeições, substituições inteligentes, montagem de cardápios e como alcançar suas metas nutricionais diárias.

                                ===== PERFIL DO USUÁRIO =====
                                Meta diária de calorias: {perfil.MetaCalorias} kcal
                                Meta diária de proteínas: {perfil.MetaProteinas} g
                                Meta diária de carboidratos: {perfil.MetaCarboidratos} g
                                Meta diária de gorduras: {perfil.MetaGorduras} g

                                ===== ALIMENTOS CONSUMIDOS HOJE =====
                                Calorias consumidas: {alimentosConsumidos.Sum(a => a.Calorias)} kcal
                                Proteínas consumidas: {alimentosConsumidos.Sum(a => a.Proteinas)} g
                                Carboidratos consumidos: {alimentosConsumidos.Sum(a => a.Carboidratos)} g
                                Gorduras consumidas: {alimentosConsumidos.Sum(a => a.Gorduras)} g

                                ===== REGRAS IMPORTANTES =====

                                1. Responda apenas perguntas relacionadas a nutrição, alimentação ou dietas.
                                2. Caso a pergunta não seja do tema, responda:
                                   “Desculpe, só posso ajudar com questões relacionadas à nutrição e alimentação. Como posso ajudá-lo com sua dieta?”
                                3. Utilize linguagem simples, objetiva e fácil de entender.
                                4. Baseie todas as orientações em evidências científicas.
                                5. Nunca ofereça dietas extremas ou restritivas demais.
                                6. Recomende acompanhamento profissional sempre que necessário.
                                7. Considere as metas nutricionais do usuário ao sugerir refeições, porções ou substituições.
                                8. Explique como cada sugestão ajuda o usuário a alcançar suas metas diárias.
                                9. Mantenha um tom encorajador, positivo e motivador.
                               10. Traga o conteúdo em texto corrido, sem listas ou tabelas.
                               11. A resposta deve ter entre 3 e 5 parágrafos curtos.
                               
                                ===== FORMATAÇÃO OBRIGATÓRIA =====
                                Você DEVE separar cada parágrafo usando duas quebras de linha.
                                É proibido entregar a resposta em um único bloco de texto.

                                ===== MENSAGEM DO USUÁRIO =====
                                {mensagemUsuario}

                                Responda de forma clara, objetiva, amigável e personalizada, considerando o perfil e as metas do usuário.
                                ";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 8192,
                        candidateCount = 1
                    },
                    safetySettings = new[]
                    {
                        new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                        new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                        new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                        new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao chamar API Gemini: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponseSugestion>();
                var textoResposta = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (string.IsNullOrEmpty(textoResposta))
                {
                    throw new Exception("A API Gemini não retornou uma resposta válida.");
                }

                return textoResposta;

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao conversar com IA: {ex.Message}");
            }
        }


        private class GeminiResponse
        {
            public List<Candidate> Candidates { get; set; }
        }

        private class Candidate
        {
            public Content Content { get; set; }
        }

        private class Content
        {
            public List<Part> Parts { get; set; }
        }

        private class Part
        {
            public string Text { get; set; }
        }

        public class GeminiResponseSugestion
        {
            public List<CandidateSugestion> Candidates { get; set; }
        }

        public class CandidateSugestion
        {
            public ContentSugestion Content { get; set; }
            public string FinishReason { get; set; }
            public int Index { get; set; }
            public List<SafetyRatingSugestion> SafetyRatings { get; set; }
        }

        public class ContentSugestion
        {
            public List<PartSugestion> Parts { get; set; }
            public string Role { get; set; }
        }

        public class PartSugestion
        {
            public string Text { get; set; }
        }

        public class SafetyRatingSugestion
        {
            public string Category { get; set; }
            public string Probability { get; set; }
        }
    }
}
