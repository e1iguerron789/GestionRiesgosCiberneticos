using Microsoft.Extensions.Configuration;
using CyberRiskManager.Models;
using System.Net.Http;
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
            var prompt = $@"
Eres un experto en ciberseguridad. Según este activo:
- Tipo: {tipoActivo}
- Confidencialidad: {c}
- Integridad: {i}
- Disponibilidad: {d}

Devuelve solo este JSON:
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
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (jsonResponse.Contains("\"error\""))
                throw new Exception("❌ Error desde Gemini: " + jsonResponse);

            var resultJsonString = JsonDocument.Parse(jsonResponse)
                .RootElement.GetProperty("candidates")[0]
                .GetProperty("content").GetProperty("parts")[0]
                .GetProperty("text").GetString();

            int first = resultJsonString.IndexOf('{');
            int last = resultJsonString.LastIndexOf('}');
            string jsonClean = resultJsonString.Substring(first, last - first + 1);

            var innerJson = JsonDocument.Parse(jsonClean);
            var amenazas = innerJson.RootElement.GetProperty("amenazas").EnumerateArray().Select(x => x.GetString()!).ToList();
            var vulnerabilidades = innerJson.RootElement.GetProperty("vulnerabilidades").EnumerateArray().Select(x => x.GetString()!).ToList();

            return (amenazas, vulnerabilidades);
        }

        public async Task<(string estrategia, string justificacion, List<string> controles)> GenerarTratamientoIAAsync(Riesgo riesgo, Activo activo)
        {
            var prompt = $@"
Eres un asesor experto en ciberseguridad. Analiza el siguiente riesgo y recomienda:

1. Recomienda la estrategia de tratamiento más adecuada entre estas segun el caso: Mitigar, Transferir, Aceptar o Evitar, basada en el tipo de activo, su criticidad y el nivel de riesgo.
2. Justifica por qué esa estrategia es la mejor opción en este contexto.
3. Sugiere 5 controles específicos que deberían implementarse. Para cada uno:
   - Explica la acción concreta.
   - Indica qué problema o riesgo específico ayuda a mitigar o reducir.

Información del riesgo:
- Activo: {activo.Nombre} (Tipo: {activo.Tipo})
- Confidencialidad: {activo.Confidencialidad}, Integridad: {activo.Integridad}, Disponibilidad: {activo.Disponibilidad}
- Amenaza: {riesgo.Amenaza}
- Vulnerabilidad: {riesgo.Vulnerabilidad}
- Nivel de riesgo: {riesgo.NivelRiesgo} (Prioridad: {riesgo.Prioridad})

Devuelve solo un JSON válido, sin comentarios ni código, con esta estructura:

{{
  ""estrategia"": ""(una de: Mitigar, Transferir, Aceptar, Evitar)"",
  ""justificacion"": ""Texto explicativo de por qué se eligió esa estrategia."",
  ""controles"": [
    {{
      ""accion"": ""Descripción del control a implementar."",
      ""problema_resuelve"": ""Qué riesgo o situación soluciona.""
    }}
  ]
}}
";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (jsonResponse.Contains("\"error\""))
                throw new Exception("❌ Error desde Gemini: " + jsonResponse);

            try
            {
                var rawText = JsonDocument.Parse(jsonResponse)
                    .RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (rawText.Contains("```json"))
                {
                    int start = rawText.IndexOf('{');
                    int end = rawText.LastIndexOf('}');
                    rawText = rawText.Substring(start, end - start + 1);
                }

                var doc = JsonDocument.Parse(rawText);
                var root = doc.RootElement;

                var estrategia = root.GetProperty("estrategia").GetString() ?? "";
                var justificacion = root.GetProperty("justificacion").GetString() ?? "";

                var controles = new List<string>();
                foreach (var ctrl in root.GetProperty("controles").EnumerateArray())
                {
                    var accion = ctrl.GetProperty("accion").GetString();
                    var problema = ctrl.GetProperty("problema_resuelve").GetString();
                    controles.Add($" {accion}\n {problema}");
                }

                return (estrategia, justificacion, controles);
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Error procesando la respuesta de la IA: " + ex.Message + "\nRespuesta completa:\n" + jsonResponse);
            }
        }
    }
}
