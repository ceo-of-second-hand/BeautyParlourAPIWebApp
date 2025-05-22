using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeautyParlourAPIWebApp.Models
{
    public class Hashtag
    {
        public int Id { get; set; }
        public string Tag { get; set; }

        [JsonIgnore]
        public ICollection<PortfolioItemHashtag> PortfolioItemHashtags { get; set; }
    }

    public class PortfolioItemHashtag
    {
        [Key]
        public int Id { get; set; }
        public int PortfolioItemId { get; set; }
        public PortfolioItem PortfolioItem { get; set; }
        public int HashtagId { get; set; }
        public Hashtag Hashtag { get; set; }
    }
} 