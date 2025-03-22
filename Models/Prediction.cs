using Newtonsoft.Json;

namespace RoadDefectsDetection.Server.Models
{
    public class Prediction
    {
        [JsonProperty(PropertyName = "x")]
        public double X { get; set; } // X koordinatı

        [JsonProperty(PropertyName = "y")]
        public double Y { get; set; } // Y koordinatı

        [JsonProperty(PropertyName = "width")]
        public double Width { get; set; } // Genişlik

        [JsonProperty(PropertyName = "height")]
        public double Height { get; set; } // Yükseklik

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; } // Güven puanı

        [JsonProperty(PropertyName = "class")]
        public string Class { get; set; } // Sınıf

        [JsonProperty(PropertyName = "class_id")]
        public int ClassId { get; set; } // Sınıf ID'si

        [JsonProperty(PropertyName = "detection_id")]
        public string DetectionId { get; set; } // nesne ID'si

    }
}
