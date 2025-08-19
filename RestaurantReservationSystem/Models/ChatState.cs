namespace RestaurantReservationSystem.Models
{
    public class ChatState
    {
        public string? Action {  get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public int? PartySize {  get; set; }
        public string? Name { get; set; }
        public string? Phone {  get; set; }

        public string? Response { get; set; }
    }
}
