using Microsoft.Extensions.Configuration;
using NutriTrack_Domains.Interfaces.AiConnection;
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

        public async Task<string> ConversarSobreNutricao(string mensagemUsuario)
        {
            try
            {
                var prompt = $@"Você é um assistente nutricional especializado. Sua função é ajudar usuários com dúvidas sobre alimentação saudável, dietas, horários de refeições, sugestões de cardápios e questões nutricionais em geral.

                                REGRAS IMPORTANTES:
                                1. Responda APENAS perguntas relacionadas a nutrição, alimentação, dietas, saúde alimentar e temas correlatos
                                2. Se a pergunta NÃO for sobre nutrição/alimentação, responda educadamente: ""Desculpe, só posso ajudar com questões relacionadas a nutrição e alimentação. Como posso ajudá-lo com sua dieta?""
                                3. Seja objetivo, claro e use linguagem acessível
                                4. Forneça informações baseadas em evidências científicas
                                5. Não substitua consultas médicas - sempre recomende profissionais quando necessário
                                6. Não forneça dietas restritivas sem acompanhamento profissional

                                MENSAGEM DO USUÁRIO: {mensagemUsuario}

                                Responda de forma amigável e útil:";

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
                        maxOutputTokens = 1024,
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
    }
}
