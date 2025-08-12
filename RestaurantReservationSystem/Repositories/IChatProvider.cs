namespace RestaurantReservationSystem.Repositories
{
    public interface IChatProvider
    {
        ///<summary>
        ///Sends chat messages to a model and returns the assistant's reply text.
        /// </summary>
        
        Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, CancellationToken cancellationToken);

    }
}
