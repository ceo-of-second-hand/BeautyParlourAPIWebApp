using System.Text.Json.Serialization;

namespace BeautyParlourAPIWebApp.Models
{
    public class Stylist
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Board> Boards { get; set; }

        [JsonIgnore]
        public ICollection<Certificate> Certificates { get; set; }
    }
} 