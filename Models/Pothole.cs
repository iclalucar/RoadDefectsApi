using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoadDefectsDetection.Server.Models
{
    public class Pothole
    {

        [Key] // Anahtar tanımı
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        public double? Confidence { get; set; }

        public string? Description { get; set; }
        public string ImageBase64 { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string Location { get; set; }

        public string UserId { get; set; }

        [NotMapped]
        public string UserName { get; set; }

        [ForeignKey("UserId")]
        public UserEntity? User { get; set; } = null!;

    }
}
