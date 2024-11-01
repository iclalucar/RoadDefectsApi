using System.Text.Json.Serialization;

namespace RoadDefectsDetection.Server.Models
{
    public class User
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }
    }
}
