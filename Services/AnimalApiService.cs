// Services/AnimalApiService.cs
using PetConnect.Models.Api;
using System.Text.Json;

namespace PetConnect.Services
{
    public class AnimalApiService   
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AnimalApiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _configuration = configuration;
        }

        public async Task<List<AnimalViewModel>> ObtenerAnimalesAdopcionAsync(int cantidadPorTipo = 10)
        {
            var dogApiKey = _configuration["AnimalApis:DogApiKey"];
            var catApiKey = _configuration["AnimalApis:CatApiKey"];

            var perrosTask = ObtenerAnimalesAsync<AnimalApiResponse>(
                $"https://api.thedogapi.com/v1/images/search?limit={cantidadPorTipo}&has_breeds=1", dogApiKey, "Perro");
            
            var gatosTask = ObtenerAnimalesAsync<AnimalApiResponse>(
                $"https://api.thecatapi.com/v1/images/search?limit={cantidadPorTipo}&has_breeds=1", catApiKey, "Gato");

            await Task.WhenAll(perrosTask, gatosTask);

            var animales = perrosTask.Result.Concat(gatosTask.Result).ToList();
            
            // Mezclamos la lista para que no aparezcan todos los perros primero
            return animales.OrderBy(a => Guid.NewGuid()).ToList();
        }

        private async Task<List<AnimalViewModel>> ObtenerAnimalesAsync<T>(string url, string apiKey, string tipo)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new List<AnimalViewModel>();

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<List<AnimalApiResponse>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse == null) return new List<AnimalViewModel>();

            return apiResponse.Select(item => new AnimalViewModel
            {
                Id = item.Id,
                UrlImagen = item.Url,
                Nombre = item.Breeds.FirstOrDefault()?.Name ?? "Mestizo",
                Origen = item.Breeds.FirstOrDefault()?.Origin ?? "Desconocido",
                Temperamento = item.Breeds.FirstOrDefault()?.Temperament ?? "Amigable",
                Tipo = tipo
            }).ToList();
        }
    }
}