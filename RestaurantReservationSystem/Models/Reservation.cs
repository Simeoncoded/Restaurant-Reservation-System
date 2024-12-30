using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.Models
{
    public class Reservation
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Customer Phone is required")]
        [Display(Name = "Customer Phone")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(10)]
        public string? Phone { get; set; }

        [Display(Name = "Customer Email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please follow the correct email format test@email.com")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }


        [Required(ErrorMessage ="You cannot leave Reservation Date blank")]
        [Display(Name = "Reservation Date")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "You cannot leave Reservation Time blank")]
        [Display(Name = "Reservation Time")]
        public TimeSpan Time { get; set; }

        [Display(Name = "End Time")]
        public TimeSpan? EndTime { get; set; }


        [Required(ErrorMessage = "You cannot leave Reservation Size blank")]
        [Range(1, int.MaxValue, ErrorMessage = "Party size must be at least 1")]
        [Display(Name = "Reservation Size")]
        public int PartySize {  get; set; }

        [Display(Name = "Reservation Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed; //defaults to confirmed

        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Checked-In Status")]
        public bool IsCheckedIn { get; set; } = false; //defaults to false

        [Display(Name = "Reservation Table")]
        public int TableID { get; set; }

        public Table? Table { get; set; }
    }
}
