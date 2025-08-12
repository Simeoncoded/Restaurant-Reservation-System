namespace RestaurantReservationSystem.Repositories
{
    public interface IReservationService
    {
        Task<List<DateTime>> CheckAvailabilityAsync(DateTime requested, int partySize, CancellationToken ct = default);
        Task<CreateReservationResult> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default);
    }

    public class CreateReservationDto
    {
        public DateTime DateTime { get; set; }  
        public int PartySize { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class CreateReservationResult
    {
        public bool Success { get; set; }
        public string? ConfirmationNumber {  get; set; }
        public string? Error {  get; set; }
    }


}
