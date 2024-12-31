using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.Models
{
    public class Reservation : IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "Phone")]
        public string PhoneFormatted => "(" + Phone?.Substring(0, 3) + ") "
           + Phone?.Substring(3, 3) + "-" + Phone?[6..];


        public string Summary
        {
            get
            {
                var timeString = Time.HasValue
                    ? Date?.Add(Time.Value).ToString("hh:mm tt") ?? "N/A"
                    : "N/A";
                return $"{FirstName} {LastName} - Party of {PartySize} on {Date?.ToString("d") ?? "N/A"} at {timeString}";
            }
        }




        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "First name cannot be more than 100 characters long.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        [MaxLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
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
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "You cannot leave Reservation Time blank")]
        [Display(Name = "Reservation Time")]
        [DataType(DataType.Time)]
        public TimeSpan? Time { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; }


        [Required(ErrorMessage = "You cannot leave Reservation Size blank")]
        [Range(1, int.MaxValue, ErrorMessage = "Party size must be at least 1")]
        [Display(Name = "Reservation Size")]
        public int PartySize {  get; set; }

        [Display(Name = "Reservation Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed; //defaults to confirmed

        [Display(Name = "Special Requests")]
        [StringLength(255, ErrorMessage ="Special Requests cannot exceed 500 characters")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Checked-In Status")]
        public bool IsCheckedIn { get; set; } = false; //defaults to false

        [Display(Name = "Reservation Table")]
        public int TableID { get; set; }

        public Table? Table { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date.GetValueOrDefault() < DateTime.Today)
            {
                yield return new ValidationResult("Reservation Date cannot be in the past.", ["Date"]);
            }

            if (Time.GetValueOrDefault() < DateTime.Now.AddHours(2).TimeOfDay)
            {
                yield return new ValidationResult("Reservation Time must be at least 2 hours from now.", ["Time"]);
            }

            if(EndTime <= Time)
            {
                yield return new ValidationResult("End time must be later than the start time.", ["EndTime"]);
            }

            if (Date.GetValueOrDefault() > DateTime.Today.AddMonths(6))
            {
                yield return new ValidationResult("Reservations cannot be made more than 6 months in advance.", ["Date"]);
            }

        }
    }
}
