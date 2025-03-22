namespace RoadDefectsDetection.Server.Models
{
    public class AuthResponse
    {
        public User User { get; set; }
        public string AccessToken { get; set; }
        public string Role { get; set; }
        public Pothole Pothole { get; set; }
    }
}