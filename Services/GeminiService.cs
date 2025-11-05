using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System; // Para Exception

namespace PetConnect.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly IHttpClientFactory _httpClientFactory;

        // Clases auxiliares para leer la respuesta JSON de Gemini
        // (Puedes moverlas a un archivo separado si quieres)
        private class GeminiResponse
        {
            public Candidate[] candidates { get; set; }
            public Error error { get; set; } // Para capturar errores de la API
        }
        private class Candidate { public Content content { get; set; } }
        private class Content { public Part[] parts { get; set; } }
        private class Part { public string text { get; set; } }
        private class Error { public string message { get; set; } }


        // 1. Inyectamos IHttpClientFactory (igual que en NoticiasController)
        public GeminiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _apiKey = configuration["Gemini:ApiKey"]; // Asegúrate que coincida con tu appsettings
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GenerarBorradorDeNoticia(string titulo)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "<p>Error: La clave de API de GoogleGemini no está configurada en el servidor.</p>";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var prompt = $"Escribe un artículo de blog para un sitio web de mascotas llamado 'PetConnect'. " +
                         $"El artículo debe ser informativo, amigable y estar en español. " +
                         $"El título es: '{titulo}'. " +
                         $"Genera aproximadamente 300 palabras. " +
                         $"Formatea la respuesta usando etiquetas HTML (como <h2>, <p>, y <ul><li> si es necesario). " +
                         $"No incluyas la etiqueta <html> o <body>.";

            // 3. Este es el "body" que la API de Gemini espera (en formato JSON)
            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var cliente = _httpClientFactory.CreateClient();
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                // 4. Hacemos la llamada POST (no GET)
                var httpResponse = await cliente.PostAsync(url, content);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    return $"<p>Error de la API de Gemini ({httpResponse.StatusCode}): {errorContent}</p>";
                }

                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

                // 5. Leemos la respuesta JSON
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Si la API devuelve un error (ej. clave inválida)
                if (geminiResponse.error != null)
                {
                    return $"<p>Error de la API de Gemini: {geminiResponse.error.message}</p>";
                }

                // 6. Extraemos el texto
                string borradorHtml = geminiResponse.candidates[0].content.parts[0].text;
                return borradorHtml;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al llamar a Gemini API: {ex.Message}");
                return $"<p>Error de conexión con la IA: {ex.Message}</p>";
            }
        }

        public async Task<string> GenerarEtiquetas(string titulo, string contenidoLimpio)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "Error: API Key no configurada";
            }

            // Usamos el mismo modelo que ya confirmamos que funciona
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            // --- ¡ESTE ES EL CAMBIO PRINCIPAL! ---
            // Un prompt enfocado en clasificación y formato de salida.
            var prompt = $"Analiza el siguiente título y contenido de un artículo para un blog de mascotas. " +
                        $"Basado en el texto, sugiere un máximo de 5 etiquetas (tags) relevantes. " +
                        $"Quiero que la respuesta sea ÚNICAMENTE una lista de palabras separadas por comas, y nada más. " +
                        $"Ejemplo de respuesta: Perros, Salud, Entrenamiento, Comida\n\n" +
                        $"TÍTULO: \"{titulo}\"\n" +
                        $"CONTENIDO: \"{contenidoLimpio}\"";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var cliente = _httpClientFactory.CreateClient();
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var httpResponse = await cliente.PostAsync(url, content);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error de API (SugerirEtiquetas): {errorContent}");
                    return $"Error: {errorContent}"; // Devuelve el error para depuración
                }

                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (geminiResponse.error != null)
                {
                    return $"Error: {geminiResponse.error.message}";
                }

                string etiquetas = geminiResponse.candidates[0].content.parts[0].text;
                
                // Limpieza final por si la IA añade espacios extra
                return etiquetas.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al llamar a Gemini API (Etiquetas): {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

    }
}