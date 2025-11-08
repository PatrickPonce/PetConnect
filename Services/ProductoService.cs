// Services/ProductoService.cs
using PetConnect.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PetConnect.Services
{
    public class ProductoService
    {
        // --- API 1: CATÁLOGO SIMULADO ---
        private static readonly List<ProductoPetShop> _catalogoProductos = new List<ProductoPetShop>
        {
            new ProductoPetShop { Id = 1, Nombre = "Comida Royal Canin Perro", Descripcion = "...", Precio = 150.00m, TipoProducto = "Comida", Tags = new List<string> { "Perro" }, QueryImagen = "dry dog food" },
            new ProductoPetShop { Id = 2, Nombre = "Pelota de Juguete Resistente", Descripcion = "...", Precio = 35.50m, TipoProducto = "Juguetes", Tags = new List<string> { "Perro" }, QueryImagen = "dog toy ball" },
            new ProductoPetShop { Id = 3, Nombre = "Rascador de Gato (Torre)", Descripcion = "...", Precio = 220.00m, TipoProducto = "Accesorios", Tags = new List<string> { "Gato" }, QueryImagen = "cat tree" },
            new ProductoPetShop { Id = 4, Nombre = "Arena Sanitaria Aglomerante", Descripcion = "...", Precio = 80.00m, TipoProducto = "Higiene", Tags = new List<string> { "Gato" }, QueryImagen = "cat litter" },
            new ProductoPetShop { Id = 5, Nombre = "Collar de Cuero para Perro", Descripcion = "...", Precio = 45.00m, TipoProducto = "Accesorios", Tags = new List<string> { "Perro" }, QueryImagen = "dog collar" },
            new ProductoPetShop { Id = 6, Nombre = "Transportadora de Viaje Mediana", Descripcion = "...", Precio = 180.00m, TipoProducto = "Accesorios", Tags = new List<string> { "Viaje" }, QueryImagen = "pet carrier" }
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _pexelsApiKey;
        private readonly Dictionary<string, string> _imageCache = new Dictionary<string, string>(); // Caché simple

        public ProductoService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _pexelsApiKey = configuration["Pexels:ApiKey"];
        }

        // Método principal que obtiene productos Y sus imágenes
        public async Task<List<ProductoPetShop>> ObtenerProductosAsync()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_pexelsApiKey);

            foreach (var producto in _catalogoProductos)
            {
                // Si no tenemos la imagen en caché, la buscamos
                if (string.IsNullOrEmpty(producto.UrlImagen) && !_imageCache.ContainsKey(producto.QueryImagen))
                {
                    var url = $"https://api.pexels.com/v1/search?query={Uri.EscapeDataString(producto.QueryImagen)}&per_page=1";
                    var pexelsResponse = await client.GetFromJsonAsync<PexelsResponse>(url);

                    var imageUrl = pexelsResponse?.photos?.FirstOrDefault()?.src?.medium ?? "/images/placeholder.png";
                    _imageCache[producto.QueryImagen] = imageUrl; // Guardamos en caché
                }

                // Asignamos desde el caché (o el valor que ya tenía si fue cargado antes)
                producto.UrlImagen = _imageCache[producto.QueryImagen];
            }
            return _catalogoProductos;
        }

        public async Task<ProductoPetShop?> ObtenerProductoPorIdAsync(int id)
        {
            var producto = _catalogoProductos.FirstOrDefault(p => p.Id == id);
            if (producto == null) return null;

            // Asegurarnos de que tenga la imagen
            if (string.IsNullOrEmpty(producto.UrlImagen))
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_pexelsApiKey);

                if (!_imageCache.ContainsKey(producto.QueryImagen))
                {
                     var url = $"https://api.pexels.com/v1/search?query={Uri.EscapeDataString(producto.QueryImagen)}&per_page=1";
                    var pexelsResponse = await client.GetFromJsonAsync<PexelsResponse>(url);
                    _imageCache[producto.QueryImagen] = pexelsResponse?.photos?.FirstOrDefault()?.src?.medium ?? "/images/placeholder.png";
                }
                producto.UrlImagen = _imageCache[producto.QueryImagen];
            }

            return producto;
        }

        // Clases auxiliares para la respuesta de Pexels
        private class PexelsResponse { public List<PexelsPhoto> photos { get; set; } }
        private class PexelsPhoto { public PexelsSource src { get; set; } }
        private class PexelsSource { public string medium { get; set; } }
    }
}