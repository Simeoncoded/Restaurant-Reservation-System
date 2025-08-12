namespace RestaurantReservationSystem.Repositories
{
    public interface IReservationService
    {
        ///<summary>
        /// Returns available 30-min slots on the requested day where 
        /// at least one table can seat the party and is not booked.
        ///</summary>
        Task<List<DateTime>> CheckAvailabilityAsync(DateTime requested, int partySize, CancellationToken ct = default);

        /// <summary>
        /// Creates a reservation after validating conflicts, capacity,
        /// hours.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<CreateReservationResult> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default);
    }

    public class CreateReservationDto
    {
        public DateTime DateTime { get; set; }  
        public int PartySize { get; set; }
        public int TableID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }
        public string? SpecialRequests {  get; set; }
    }

    public class CreateReservationResult
    {
        public bool Success { get; set; }
        public int? ReservationId {  get; set; }
        public string? ConfirmationNumber {  get; set; }
        public string? Error {  get; set; }
    }


}
