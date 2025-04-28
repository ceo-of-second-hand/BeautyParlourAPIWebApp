using Microsoft.EntityFrameworkCore;

namespace BeautyParlourAPIWebApp.Models
{
    public class BeautyParlourAPIContext : DbContext
    {
        public BeautyParlourAPIContext(DbContextOptions<BeautyParlourAPIContext> options)
            : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Stylist> Stylists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
} 