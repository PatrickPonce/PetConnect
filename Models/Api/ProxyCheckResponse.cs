// Models/Api/ProxyCheckResponse.cs
using System.Text.Json.Serialization;

namespace PetConnect.Models.Api
{
    // Usamos atributos JsonPropertyName para mapear el JSON a C#
    public class ProxyCheckResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("country")]
        public string Pais { get; set; }

        [JsonPropertyName("city")]
        public string Ciudad { get; set; }

        [JsonPropertyName("proxy")]
        public string EsProxy { get; set; } // "yes" o "no"

        [JsonPropertyName("type")]
        public string Tipo { get; set; } // "VPN", "Hosting", "Residential"
    }
}