namespace RestaurantReservationSystem.Models
{
    public class Notification : Auditable
    {
        public int Id { get; set; }
        public string Message { get; set; }

        // Optional: link to a page (e.g., reservation details)
        public string Link { get; set; }

        public string UserId { get; set; } // Foreign key to AspNetUsers
        public bool IsRead { get; set; }
    }
}
