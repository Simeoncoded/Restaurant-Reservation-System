using NuGet.Protocol.Plugins;
using RestaurantReservationSystem.Data;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.Models
{
    public class Reservation : Auditable, IValidatableObject
    {
        public int ID { get; set; }


        #region Summary Properties
        [Display(Name = "Phone")]
        public string PhoneFormatted => "(" + Phone?.Substring(0, 3) + ") "
           + Phone?.Substring(3, 3) + "-" + Phone?[6..];


        [Display(Name = "Time")]
        public string TimeSummary
        {
            get
            {
                var startTimeString = Date.HasValue && Time.HasValue
                 ? Date?.Add(Time.Value).ToString("hh:mm tt") ?? "N/A"
                 : "N/A";


                return $"{startTimeString}";
            }
        }

        [Display(Name = "Customer Details")]
        public string CustomerDetails
        {
            get
            {

                return $"Phone: {PhoneFormatted}, Email: {Email} ";
            }
        }

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

        #endregion

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


        [Required(ErrorMessage = "You cannot leave Reservation Date blank")]
        [Display(Name = "Reservation Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "You cannot leave Reservation Time blank")]
        [Display(Name = "Reservation Time")]
        [DataType(DataType.Time)]
        public TimeSpan? Time { get; set; }


        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency


        [Required(ErrorMessage = "You cannot leave Reservation Size blank")]
        [Range(1, 20, ErrorMessage = "Party size must be between 1 and 20.")]
        [Display(Name = "Reservation Size")]
        public int PartySize { get; set; }

        [Display(Name = "Reservation Status")]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending; //defaults to confirmed

        [Display(Name = "Special Requests")]
        [StringLength(255, ErrorMessage = "Special Requests cannot exceed 500 characters")]
        public string? SpecialRequests { get; set; }

        [Display(Name = "Checked-In Status")]
        public bool IsCheckedIn { get; set; } = false; //defaults to false

        [Display(Name = "Reservation Table")]
        public int TableID { get; set; }

        public Table? Table { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var dbContext = (RestaurantReservationSystemContext)validationContext.GetService(typeof(RestaurantReservationSystemContext));
            var table = dbContext.Tables.FirstOrDefault(t => t.ID == TableID); // Get the correct table

            if (table != null && PartySize > table.Capacity)
            {
                yield return new ValidationResult($"Party size exceeds the table's capacity of {table.Capacity}.", new[] { "PartySize" });
            }

            if (table != null && PartySize < 1)
            {
                yield return new ValidationResult($"Party size must be at least 1 for table {TableID}.", new[] { "PartySize" });
            }

            var resOpen = new TimeSpan(10, 0, 0); // 10 AM
            var resClose = new TimeSpan(22, 0, 0); // 10 PM

            if (Time < resOpen || Time > resClose)
            {
                yield return new ValidationResult($"Reservation Time must be between {resOpen:hh\\:mm} and {resClose:hh\\:mm}.", new[] { "Time" });
            }

            if (Date.GetValueOrDefault() < DateTime.Today)
            {
                yield return new ValidationResult("Reservation Date cannot be in the past.", new[] { "Date" });
            }

            if (Date.HasValue && Time.HasValue)
            {
                var reservationDateTime = Date.Value.Add(Time.Value); // Combine date and time
                var tableReservations = dbContext.Reservations
                    .Where(r => r.TableID == TableID && r.Date == Date && r.ID != ID) // Exclude the current reservation by ID
                    .ToList();

                foreach (var existingReservation in tableReservations)
                {
                    var existingStartTime = existingReservation.Date.Value.Add(existingReservation.Time.Value);
                    var existingEndTime = existingStartTime.AddHours(1); // Assuming 1-hour slots
                    var newReservationEndTime = reservationDateTime.AddHours(1);

                    if (reservationDateTime < existingEndTime && newReservationEndTime > existingStartTime)
                    {
                        yield return new ValidationResult($"The selected table is already booked from {existingStartTime:hh:mm tt} to {existingEndTime:hh:mm tt}. Please select a time after {existingEndTime:hh:mm tt}.", new[] { "Time" });
                    }
                }
            }

            if (Date.GetValueOrDefault() > DateTime.Today.AddMonths(6))
            {
                yield return new ValidationResult("Reservations cannot be made more than 6 months in advance.", new[] { "Date" });
            }
        }
    }
}
