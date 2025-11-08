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

    /*
    public class AiConnectionService : IAiConnectionService
    {
        private readonly HttpClient _client;

        public AiConnectionService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GeminiConnection(string pergunta)
        {
            var apiKey = "AIzaSyDV6rF_iCn5Wkq89dcf2CpM7KCyttFnpiU";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/" +
                      $"gemini-2.5-flash-preview-09-2025:generateContent?key={apiKey}";

            var requestObj = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = pergunta }
                        }
                    }
                }
            };

            var response = await _client.PostAsJsonAsync(url, requestObj);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro da API Gemini: {json}");

            var result = System.Text.Json.JsonSerializer.Deserialize<GeminiResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            return CleanResponse(result.candidates[0].content.parts[0].text);
        }


        //método para limpeza do retorno
        private static string CleanResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return response;

            response = response.Replace("**", "");

            response = System.Text.RegularExpressions.Regex.Replace(response, @"\$(.*?)\$", "");

            response = System.Text.RegularExpressions.Regex.Replace(response, @"\\\(.*?\\\)", "");
            response = System.Text.RegularExpressions.Regex.Replace(response, @"\\\[.*?\\\]", "");

            response = System.Text.RegularExpressions.Regex.Replace(response, @"\s+", " ");

            response = response.Trim();

            return response;
        }
    }

    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
        public Usagemetadata usageMetadata { get; set; }
        public string modelVersion { get; set; }
        public string responseId { get; set; }
    }

    public class Usagemetadata
    {
        public int promptTokenCount { get; set; }
        public int candidatesTokenCount { get; set; }
        public int totalTokenCount { get; set; }
        public Prompttokensdetail[] promptTokensDetails { get; set; }
    }

    public class Prompttokensdetail
    {
        public string modality { get; set; }
        public int tokenCount { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
        public string finishReason { get; set; }
        public int index { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
        public string role { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
    */

}
