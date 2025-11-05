using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PetConnect.Services
{
    public class PerspectiveService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public PerspectiveService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Perspective:ApiKey"]; // Lee la clave desde appsettings.json
        }

        // Analiza el texto y devuelve TRUE si es tóxico
        public async Task<bool> EsComentarioToxico(string texto)
    {
        // 1. BAJA EL UMBRAL
        const double TOXICITY_THRESHOLD = 0.7; 

        // Verifica si la clave se cargó (¡MUY IMPORTANTE!)
        if (string.IsNullOrEmpty(_apiKey))
        {
            Console.WriteLine("[Perspective API] ERROR: La clave de API es NULA o no se encontró en appsettings.json. Revisa la ruta 'Perspective:ApiKey'.");
            return false; // Falla en modo abierto
        }

        var apiUrl = $"https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key={_apiKey}";

        var requestBody = new
        {
            comment = new { text = texto },
            languages = new[] { "es" },
            requestedAttributes = new { TOXICITY = new {} }
        };

        var jsonRequest = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                // 2. AÑADE LOGS PARA VER ERRORES DE API
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Perspective API] ERROR: La API devolvió {response.StatusCode}.");
                Console.WriteLine($"[Perspective API] Respuesta de error: {errorBody}");
                // (Si aquí ves "API key not valid" o "API not enabled", ese es tu problema)
                return false; 
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var parsedResponse = JObject.Parse(jsonResponse);

            var toxicityScore = parsedResponse["attributeScores"]?["TOXICITY"]?["summaryScore"]?["value"]?.Value<double>();

            if (toxicityScore.HasValue)
            {
                // 3. AÑADE UN LOG PARA VER LA PUNTUACIÓN
                Console.WriteLine($"[Perspective API] Puntuación para '{texto}': {toxicityScore.Value}");
                
                return toxicityScore.Value > TOXICITY_THRESHOLD;
            }
        }
        catch (Exception ex)
        {
            // 4. AÑADE UN LOG PARA VER EXCEPCIONES
            Console.WriteLine($"[Perspective API] Excepción en servicio: {ex.Message}");
        }

        return false;
    }
        }
}