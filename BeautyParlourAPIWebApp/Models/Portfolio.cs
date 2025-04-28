namespace BeautyParlourAPIWebApp.Models
{
    public class Portfolio
    {
        public int Id { get; set; }
        public int StylistId { get; set; }
        public Stylist Stylist { get; set; }
        public ICollection<PortfolioItem> Items { get; set; }
    }

    public class PortfolioItem
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
} 