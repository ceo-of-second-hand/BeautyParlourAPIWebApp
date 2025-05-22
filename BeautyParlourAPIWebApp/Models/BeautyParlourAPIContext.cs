using Microsoft.EntityFrameworkCore;

namespace BeautyParlourAPIWebApp.Models
{
    public class BeautyParlourAPIContext : DbContext
    {
        public BeautyParlourAPIContext(DbContextOptions<BeautyParlourAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Stylist> Stylists { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<PortfolioItemHashtag> PortfolioItemHashtags { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
    }
} 