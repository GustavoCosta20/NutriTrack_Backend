using Microsoft.Extensions.Configuration;
using NutriTrack_Domains.Dtos;
using NutriTrack_Domains.Interfaces.AiConnection;
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

        public AiConnectionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini API Key não configurada no appsettings.json");
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/" + $"gemini-2.5-flash-preview-09-2025:generateContent?key={_apiKey}";
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

                var response = await _httpClient.PostAsJsonAsync(_apiUrl,requestBody);

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
                var prompt = $@"
                                Você é um assistente nutricional especializado.

                                Sua missão é ajudar o usuário com orientações sobre alimentação saudável, horários, substituições inteligentes de alimentos, montagem de cardápios e como atingir suas metas nutricionais diárias.

                                ===== PERFIL DO USUÁRIO =====
                                Meta diária de calorias: {perfil.MetaCalorias} kcal
                                Meta diária de proteínas: {perfil.MetaProteinas} g
                                Meta diária de carboidratos: {perfil.MetaCarboidratos} g
                                Meta diária de gorduras: {perfil.MetaGorduras} g

                                ===== REGRAS IMPORTANTES =====
                                1. Responda APENAS perguntas relacionadas a nutrição, hábitos alimentares ou dietas.
                                2. Caso a pergunta não seja relacionada ao tema, responda apenas:
                                   ""Desculpe, só posso ajudar com questões relacionadas a nutrição e alimentação. Como posso ajudá-lo com sua dieta?""
                                3. Explique usando linguagem simples e objetiva.
                                4. Baseie as recomendações em evidências científicas.
                                5. Não forneça dietas extremamente restritivas.
                                6. Sempre recomende acompanhamento profissional quando necessário.
                                7. Leve sempre em consideração as metas nutricionais do usuário ao sugerir refeições, substituições e porções.
                                8. Quando fizer sugestões, explique como elas contribuem para atingir as metas diárias.

                                ===== MENSAGEM DO USUÁRIO =====
                                {mensagemUsuario}

                                Responda de forma clara, objetiva, amigável e personalizada com base nas metas do usuário.
                                Forneça respostas curtas, diretas e com no máximo 5 parágrafos.
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
