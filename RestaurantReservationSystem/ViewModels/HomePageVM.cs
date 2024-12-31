using RestaurantReservationSystem.Models;

namespace RestaurantReservationSystem.ViewModels
{
    public class HomePageVM
    {
        public int TotalReservationsToday { get; set; }
        public int AvailableTables { get; set; }
        public int ReservationsThisWeek { get; set; }
        public List<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
    }
}
