// Services/GoogleSearchService.cs
using Microsoft.Extensions.Configuration;
using PetConnect.Models.Api;
using PetConnect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PetConnect.Services
{
    public class GoogleSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _searchEngineId;

        public GoogleSearchService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GoogleSearch:ApiKey"] ?? "";
            _searchEngineId = configuration["GoogleSearch:SearchEngineId"] ?? "";
        }

        public async Task<List<ProductoViewModel>> BuscarProductosExternos(string marca)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_searchEngineId))
            {
                // Si faltan las claves, devolvemos lista vacía para no romper la app
                return new List<ProductoViewModel>();
            }

            var client = _httpClientFactory.CreateClient();
            // Buscamos específicamente productos de esa marca
            string query = $"productos {marca}"; 

            // URL de la API de Custom Search
            var url = $"https://www.googleapis.com/customsearch/v1?" +
                      $"key={_apiKey}&" +
                      $"cx={_searchEngineId}&" +
                      $"q={Uri.EscapeDataString(query)}&" +
                      $"num=10"; // Pedimos 10 resultados

            try
            {
                var response = await client.GetFromJsonAsync<GoogleSearchResponse>(url);

                if (response?.Items == null) return new List<ProductoViewModel>();

                // Mapeamos los resultados de Google a nuestro ViewModel unificado
                return response.Items.Select(item => new ProductoViewModel
                {
                    EsExterno = true, // ¡Importante! Marcamos como externo
                    Nombre = item.Title,
                    UrlExterna = item.Link, // Enlace a la tienda
                    // Intentamos obtener la imagen del PageMap, o usamos placeholder
                    UrlImagen = item.Pagemap?.CseImage?.FirstOrDefault()?.Src ?? "/images/placeholder.png",
                    NombreTienda = item.DisplayLink,
                    Precio = 0 // Google no nos da el precio fácilmente
                }).ToList();
            }
            catch
            {
                // En caso de error (ej. sin conexión), devolvemos lista vacía
                return new List<ProductoViewModel>();
            }
        }
    }
}