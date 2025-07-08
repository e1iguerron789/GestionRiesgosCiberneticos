using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CyberRiskManager.Services
{
    public class IAService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public IAService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Falta la clave de API de Gemini.");
            _httpClient = new HttpClient();
        }

        public async Task<(List<string> amenazas, List<string> vulnerabilidades)> SugerirAmenazasYVulnerabilidadesAsync(
            string tipoActivo, int c, int i, int d)
        {
            // Texto del prompt
            var prompt = $@"
Eres un experto en ciberseguridad y analisas las posibles amenazas y vulnerabilidades de forma experta. Según este activo:
- Tipo: {tipoActivo}
- Confidencialidad: {c}
- Integridad: {i}
- Disponibilidad: {d}

Devuelve solo un JSON así:
{{
  ""amenazas"": [""Amenaza 1"", ""Amenaza 2"", ""Amenaza 3""],
  ""vulnerabilidades"": [""Vulnerabilidad 1"", ""Vulnerabilidad 2"", ""Vulnerabilidad 3""]
}}";

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
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // 👀 Para depuración: imprime la respuesta completa
            Console.WriteLine("🔎 RESPUESTA COMPLETA:");
            Console.WriteLine(jsonResponse);

            try
            {
                var resultJsonString = JsonDocument.Parse(jsonResponse)
                    .RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(resultJsonString))
                    throw new Exception("La respuesta de Gemini está vacía.");

                int firstBrace = resultJsonString.IndexOf('{');
                int lastBrace = resultJsonString.LastIndexOf('}');
                if (firstBrace == -1 || lastBrace == -1 || lastBrace <= firstBrace)
                    throw new Exception("No se encontró JSON válido en el texto.");

                string jsonClean = resultJsonString.Substring(firstBrace, lastBrace - firstBrace + 1);

                var innerJson = JsonDocument.Parse(jsonClean);
                var amenazas = innerJson.RootElement.GetProperty("amenazas").EnumerateArray().Select(x => x.GetString()!).ToList();
                var vulnerabilidades = innerJson.RootElement.GetProperty("vulnerabilidades").EnumerateArray().Select(x => x.GetString()!).ToList();

                return (amenazas, vulnerabilidades);
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Error al analizar la respuesta: " + ex.Message + "\nRespuesta completa:\n" + jsonResponse);
            }
        }
    }
}
