using NutriTrack_Domains.Interfaces.AiConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NutriTrack_Services.IaConexionService
{
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

}
