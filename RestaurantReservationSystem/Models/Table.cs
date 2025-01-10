using System.ComponentModel.DataAnnotations;

namespace RestaurantReservationSystem.Models
{
    public class Table : Auditable
    {
        public int ID { get; set; }
            
        public string Summary
        {
            get
            {
               
                return $"{TableNumber} - Capacity: {Capacity} - Location: {Location ?? "N/A"}";
            }
        }



        [Display(Name = "Table Number")]
        [Required(ErrorMessage = "You cannot leave Table Number blank")]
        public int TableNumber {  get; set; }

        [Display(Name = "Table Capacity")]
        [Required(ErrorMessage = "You cannot leave Table Capacity blank")]
        public int Capacity {  get; set; }

        [Display(Name = "Table Status")]
        [Required(ErrorMessage = "You must select a table status")]
        public TableStatus Status { get; set; } = TableStatus.Available; //defaults to available

        [Display(Name = "Table Location")]
        [Required(ErrorMessage = "You cannot leave Table Location blank")]
        public string? Location { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[]? RowVersion { get; set; }//Added for concurrency


        public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    }
}
