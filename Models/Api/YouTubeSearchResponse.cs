// Models/Api/YouTubeSearchResponse.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PetConnect.Models.Api
{
    public class YouTubeSearchResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeItem> Items { get; set; } = new List<YouTubeItem>();
    }

    public class YouTubeItem
    {
        [JsonPropertyName("id")]
        public YouTubeId Id { get; set; }
    }

    public class YouTubeId
    {
        [JsonPropertyName("videoId")]
        public string VideoId { get; set; }
    }
}