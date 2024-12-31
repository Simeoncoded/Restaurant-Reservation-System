using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Data;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.ViewModels;
using System.Diagnostics;

namespace RestaurantReservationSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly RestaurantReservationSystemContext _context;

        public HomeController(ILogger<HomeController> logger, RestaurantReservationSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            var model = new HomePageVM
            {
                TotalReservationsToday = await _context.Reservations
                    .Where(r => r.Date == today)
                    .CountAsync(),

                // Count tables that are available
                AvailableTables = await _context.Tables
                    .Where(t => t.Status == TableStatus.Available) // Filter by Available status
                    .CountAsync(),

                ReservationsThisWeek = await _context.Reservations
                    .Where(r => r.Date >= today && r.Date <= nextWeek)
                    .CountAsync(),

                UpcomingReservations = await _context.Reservations
                    .Where(r => r.Date >= today)
                    .OrderBy(r => r.Date)
                    .Take(5) // Show top 5 upcoming reservations
                    .ToListAsync()
            };

            return View(model);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
