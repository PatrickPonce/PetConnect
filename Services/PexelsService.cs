// Services/PexelsService.cs
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PetConnect.Services
{
    public class PexelsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _pexelsApiKey;
        private readonly Dictionary<string, string> _imageCache = new Dictionary<string, string>();

        public PexelsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _pexelsApiKey = configuration["Pexels:ApiKey"];
        }

        public async Task<string> ObtenerImagenAsync(string queryImagen)
        {
            if (_imageCache.ContainsKey(queryImagen))
            {
                return _imageCache[queryImagen];
            }

            if (string.IsNullOrEmpty(_pexelsApiKey))
            {
                return "/images/placeholder.png"; 
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_pexelsApiKey);

            try
            {
                var url = $"https://api.pexels.com/v1/search?query={Uri.EscapeDataString(queryImagen)}&per_page=1";
                var pexelsResponse = await client.GetFromJsonAsync<PexelsResponse>(url);

                var imageUrl = pexelsResponse?.photos?.FirstOrDefault()?.src?.medium ?? "/images/placeholder.png";
                _imageCache[queryImagen] = imageUrl; 
                return imageUrl;
            }
            catch (Exception)
            {
                return "/images/placeholder.png"; 
            }
        }

        private class PexelsResponse { public List<PexelsPhoto> photos { get; set; } }
        private class PexelsPhoto { public PexelsSource src { get; set; } }
        private class PexelsSource { public string medium { get; set; } }
    }
}