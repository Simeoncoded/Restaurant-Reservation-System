using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.Models
{
    public enum TableStatus
    {
        Available,
        Occupied,
        Reserved,
        Unavailable,
        [Display(Name ="Under Maintenance")]
        UnderMaintenance
    }
}
