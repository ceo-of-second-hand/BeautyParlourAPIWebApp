using System.Text.Json.Serialization;

namespace BeautyParlourAPIWebApp.Models
{
    public class PortfolioItem
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; }
        public string? BeforePhotoUrl { get; set; }
        public string AfterPhotoUrl { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public ICollection<PortfolioItemHashtag> PortfolioItemHashtags { get; set; }
    }
} 