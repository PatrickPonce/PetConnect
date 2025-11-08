// Services/ChatbotService.cs

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PetConnect.Services
{
    public class ChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public ChatbotService(IConfiguration configuration)
        {
            // Leemos la clave desde la nueva sección "MiGemini"
            _apiKey = configuration["MiGemini:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("La ApiKey de 'MiGemini' no está configurada.");
            }
            
            _httpClient = new HttpClient();
            
            // Usamos el modelo 'gemini-pro' que es el estándar para texto.
            _apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-latest:generateContent?key={_apiKey}";
        }

        public async Task<string> GenerarRespuestaAsync(string promptUsuario)
        {
            // El prompt del sistema para darle personalidad a la IA
            var systemInstruction = @"Eres 'Patitas', el asistente virtual experto de PetConnect, una plataforma para amantes de las mascotas en Perú. 
                Tu especialidad son las veterinarias, adopción, pet shops y lugares pet-friendly.
                Tu tono debe ser amigable, servicial y empático. Usa emojis de animales cuando sea apropiado 🐾.
                NO debes dar consejos médicos veterinarios. Si te preguntan por síntomas, debes recomendar SIEMPRE 'consultar con una veterinaria profesional de nuestro directorio'.
                Basa tus respuestas en el contexto de Perú. Si no sabes una respuesta, di que estás aprendiendo y que recomendarías buscar en el directorio del sitio.";

            // Estructura del cuerpo de la petición para la API de Gemini
            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = systemInstruction },
                            new { text = $"Usuario: {promptUsuario}" }
                        }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_apiUrl, content);

                var responseString = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    // Si falla, imprimimos el error exacto que nos da Google
                    Console.WriteLine($"Error de la API de Gemini: {responseString}");
                    return "¡Uy! Hubo un problema al contactar mi cerebro de IA. Por favor, revisa la consola del servidor para más detalles.";
                }

                // Parseamos la respuesta exitosa para extraer el texto
                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    // Navegamos la estructura del JSON para obtener la respuesta
                    JsonElement candidates;
                    if (doc.RootElement.TryGetProperty("candidates", out candidates) && candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                                   .GetProperty("content")
                                   .GetProperty("parts")[0]
                                   .GetProperty("text")
                                   .GetString();
                        return text ?? "No pude generar una respuesta. Inténtalo de nuevo.";
                    }
                }

                return "Recibí una respuesta inesperada de la IA. Inténtalo de nuevo.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al llamar a la API de Gemini: {ex.ToString()}");
                return "¡Uy! Parece que mis circuitos se enredaron 🐾. Intenta preguntarme de nuevo en un momento.";
            }
        }
    }
}