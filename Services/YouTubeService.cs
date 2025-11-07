// Services/YouTubeService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PetConnect.Models.Api; // Para la respuesta de YouTube

namespace PetConnect.Services
{
    public class YouTubeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public YouTubeService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["YouTube:ApiKey"];
        }

        // Busca videos y devuelve una lista de IDs de video (ej. "dQw4w9WgXcQ")
        public async Task<List<string>> BuscarVideosAsync(string nombreProducto)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                // Si no hay clave de API, devuelve una lista vacía
                return new List<string>();
            }

            var client = _httpClientFactory.CreateClient();
            string consulta = $"reseña {nombreProducto}"; // Buscamos "reseña [Nombre del Producto]"

            var url = $"https://www.googleapis.com/youtube/v3/search?" +
                      $"part=snippet&" +
                      $"q={Uri.EscapeDataString(consulta)}&" +
                      $"key={_apiKey}&" +
                      $"type=video&" +
                      $"relevanceLanguage=es&" + // Damos prioridad a resultados en español
                      $"maxResults=3"; // Traemos los 3 videos principales

            try
            {
                var response = await client.GetFromJsonAsync<YouTubeSearchResponse>(url);

                return response?.Items?
                    .Select(item => item.Id.VideoId)
                    .ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                // (Opcional) Loggear el error ex.Message
                return new List<string>(); // Devolver lista vacía en caso de error
            }
        }
    }
}