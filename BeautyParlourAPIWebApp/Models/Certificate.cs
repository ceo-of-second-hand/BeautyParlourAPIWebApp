namespace BeautyParlourAPIWebApp.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        public int StylistId { get; set; }
        public Stylist Stylist { get; set; }
        public string CertificateUrl { get; set; }
        public string Description { get; set; }
        public DateTime DateUploaded { get; set; }
    }
} 