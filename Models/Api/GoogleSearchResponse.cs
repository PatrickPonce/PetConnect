// Models/Api/GoogleSearchResponse.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PetConnect.Models.Api
{
    public class GoogleSearchResponse
    {
        [JsonPropertyName("items")]
        public List<GoogleResultItem>? Items { get; set; }
    }

    public class GoogleResultItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("displayLink")]
        public string DisplayLink { get; set; } = string.Empty;

        [JsonPropertyName("pagemap")]
        public PageMap? Pagemap { get; set; }
    }

    public class PageMap
    {
        [JsonPropertyName("cse_image")]
        public List<CseImage>? CseImage { get; set; }
    }

    public class CseImage
    {
        [JsonPropertyName("src")]
        public string Src { get; set; } = string.Empty;
    }
}