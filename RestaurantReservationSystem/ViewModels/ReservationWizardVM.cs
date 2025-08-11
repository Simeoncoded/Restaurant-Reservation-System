using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.ViewModels
{
    public class ReservationWizardVM
    {
        //Step 1 -  Guest   
        [Required]
        public string? FirstName {  get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required, Phone, RegularExpression("^\\d{10}$")]
        public string? Phone {  get; set; }
        [EmailAddress]
        public string? Email { get; set; }


        //step 2 - Details
        [Required]
        public DateTime? Date { get; set; }
        [Required]
        public TimeSpan? Time { get; set; }
        [Range(1, 50)]
        public int? PartySize { get; set; }
        [Required]
        public int? TableID {  get; set; }

        //step 3 - Extras
        [StringLength (255)]
        public string? SpecialRequests {  get; set; }

        //Internal
        public int Step { get; set; } = 1;
    }
}
