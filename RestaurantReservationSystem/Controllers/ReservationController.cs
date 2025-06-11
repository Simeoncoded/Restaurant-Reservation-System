using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestaurantReservationSystem.CustomControllers;
using RestaurantReservationSystem.Data;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.Utilities;
using Microsoft.AspNetCore.SignalR;
using RestaurantReservationSystem.Hubs;

namespace RestaurantReservationSystem.Controllers
{
    public class ReservationController : ElephantController
    {
         private readonly RestaurantReservationSystemContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ReservationController(RestaurantReservationSystemContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Reservation
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? SearchString, string? PhoneString,
            string? actionButton, string sortDirection = "asc", string sortField = "Summary")
        {
            string[] sortOptions = new[] { "Summary" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            var reservations = from r in _context.Reservations
                .Include(r => r.Table)
                .AsNoTracking()
                select r;

            if (!String.IsNullOrEmpty(SearchString))
            {
               reservations = reservations.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }

            if (!String.IsNullOrEmpty(PhoneString))
            {
                reservations = reservations.Where(p => p.Phone.Contains(PhoneString));
                numberFilters++;
            }


            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                @ViewData["ShowFilter"] = " show";
            }

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;
                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }


            if (sortField == "Summary")
            {
                if (sortDirection == "asc")
                {
                    reservations = reservations
                      .OrderBy(p => p.LastName)
                      .ThenBy(p => p.FirstName);
                }
                else
                {
                    reservations = reservations
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
          
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            var tables = _context.Tables.ToList();

            ViewBag.Tables = tables; // Pass the list of tables to the view //Handle Paging

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Reservation>.CreateAsync(reservations.AsNoTracking(), page ?? 1, pageSize);


            return View(pagedData);
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservation/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Reservation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,Phone,Email,Date,Time,PartySize,Status,SpecialRequests,IsCheckedIn,TableID")] Reservation reservation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();

                    // Build the link to reservation details
                    string link = Url.Action("Details", "Reservation", new { id = reservation.ID }, Request.Scheme);

                    // Create and save a notification
                    var notification = new Notification
                    {
                        Message = $"New reservation for {reservation.FirstName} {reservation.LastName} at {reservation.Time}",
                        Link = link,
                        IsRead = false,
                        UserId = null // global notification
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // Send notification via SignalR to all connected clients
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        message = notification.Message,
                        link = notification.Link
                    });


                  
                    return RedirectToAction("Details", new { reservation.ID });
                }
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Reservations.Date"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, you cannot have duplicate Reservations(Time, Date, and Table)");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(reservation);
            return View(reservation);
        }


        // GET: Reservation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
               .FirstOrDefaultAsync(r => r.ID == id);

            if (reservation == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(reservation);
            return View(reservation);
        }

        // POST: Reservation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var reservationToUpdate = await _context.Reservations
                .Include(r => r.Table) 
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservationToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Reservation>(reservationToUpdate, "",
                r => r.FirstName, r => r.LastName, r => r.Phone, r => r.Email,
                r => r.Date, r => r.Time, r => r.PartySize, r => r.Status,
                r => r.SpecialRequests, r => r.IsCheckedIn, r => r.TableID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Reservation successfully updated.";
                    return RedirectToAction("Details", new { id = reservationToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException )
                {
                    ModelState.AddModelError("", "Concurrency Error");
                }
                catch (DbUpdateException ex)
                {
                    var baseMessage = ex.GetBaseException().Message;
                    if (baseMessage.Contains("UNIQUE") && baseMessage.Contains("Reservations.Date"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Duplicate reservation (Date, Time, Table) is not allowed.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again later.");
                    }
                }
            }

            // Repopulate dropdowns if save fails
            PopulateDropDownLists(reservationToUpdate);
            return View(reservationToUpdate);
        }



        // GET: Reservation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations
                .Include(p => p.Table)
                 .FirstOrDefaultAsync(m => m.ID == id);

            try { 
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
         catch (DbUpdateException)
            {
                
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(reservation);
}

        // GET: Reservation/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Table) 
                .FirstOrDefaultAsync(r => r.ID == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation); // Show the confirmation 
        }


        //Cancel Action
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            // Update the status 
            reservation.Status = ReservationStatus.Cancelled;

            // Save changes 
            await _context.SaveChangesAsync();

            // Redirect back 
            return RedirectToAction(nameof(Index));
        }



        private void PopulateDropDownLists(Reservation? reservation = null)
        {
            var dQuery = from d in _context.Tables
                         where d.Status == TableStatus.Available //show only available tables
                         orderby d.Location
                         select d;
            ViewData["TableID"] = new SelectList(dQuery, "ID", "Summary", reservation?.TableID );
        }
        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ID == id);
        }
    }
}
