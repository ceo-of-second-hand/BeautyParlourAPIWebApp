namespace BeautyParlourAPIWebApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int StylistId { get; set; }
        public Stylist Stylist { get; set; }
        public decimal Deposit { get; set; }
        public string PhotoReferenceUrl { get; set; }
        public bool AddedToCalendar { get; set; }
    }
} 