using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace RoadDefectsDetection.Server.Models
{
    public class RoboflowResponse
    {
        [JsonProperty(PropertyName = "predictions")]
        public List<Prediction> Predictions { get; set; } // Tahminlerin listesi
    }
}