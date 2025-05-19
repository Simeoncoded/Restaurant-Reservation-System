using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Data;

namespace RestaurantReservationSystem.Controllers
{
    public class NotificationController : Controller
    {
        private readonly RestaurantReservationSystemContext _context;

        public NotificationController(RestaurantReservationSystemContext context)
        {
            _context = context;
        }

        public IActionResult GetAll()
        {
            var notifications = _context.Notifications
                .Where(n => !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)  //list first 10 according to the time created
                .ToList();

            return PartialView("~/Views/Shared/_NotificationList.cshtml", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
