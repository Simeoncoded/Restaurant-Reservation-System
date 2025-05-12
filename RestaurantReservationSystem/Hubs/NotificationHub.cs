using Microsoft.AspNetCore.SignalR;

namespace RestaurantReservationSystem.Hubs
{
    public class NotificationHub : Hub
    {
        // This will be updated later to send to a specific user
        public async Task SendToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
