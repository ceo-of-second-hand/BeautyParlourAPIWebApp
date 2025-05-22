using System.Text.Json.Serialization;

namespace BeautyParlourAPIWebApp.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? StylistId { get; set; }
        public string? TitleImageUrl { get; set; }
        public string? ThemeColor { get; set; }

        public ICollection<PortfolioItem> Items { get; set; }
    }
} 