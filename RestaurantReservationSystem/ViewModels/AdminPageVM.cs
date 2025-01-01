using RestaurantReservationSystem.Models;

namespace RestaurantReservationSystem.ViewModels
{
    public class AdminPageVM
    {
        public int TotalReservationsToday { get; set; }
        public int AvailableTables { get; set; }
        public int ReservationsThisWeek { get; set; }
        public List<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
        public int CheckedInReservations { get; set; } // New metric for admin view
        public int TotalTables { get; set; } // Include total tables for management overview
    }
}
