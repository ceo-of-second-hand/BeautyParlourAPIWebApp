namespace BeautyParlourAPIWebApp.Models
{
    public class Stylist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public Portfolio Portfolio { get; set; }
    }
} 