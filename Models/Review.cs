namespace BeautyParlourAPIWebApp.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int StylistId { get; set; }
        public Stylist Stylist { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public DateTime Date { get; set; }
    }
} 