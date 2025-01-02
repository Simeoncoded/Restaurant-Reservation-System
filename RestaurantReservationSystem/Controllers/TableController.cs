using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Data;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.Utilities;
using RestaurantReservationSystem.ViewModels;

namespace RestaurantReservationSystem.Controllers
{
    public class TableController : Controller
    {
        private readonly RestaurantReservationSystemContext _context;

        public TableController(RestaurantReservationSystemContext context)
        {
            _context = context;
        }

        // GET: Table
        public async Task<IActionResult> Index(string? TableNum, string? TableCap, string? TableLoc, string? StatusFilter)
        {
            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //SelectList for the Coverage Enum
            if (Enum.TryParse(StatusFilter, out TableStatus selectedStatus))
            {
                ViewBag.StatusSelectList = TableStatus.Available.ToSelectList(selectedStatus);
            }
            else
            {
                ViewBag.StatusSelectList = TableStatus.Available.ToSelectList(null);
            }

            var tables = from t in _context.Tables
                         .AsNoTracking()
                         select t;

            if(TableNum != null)
            {
                tables = tables.Where(t => t.TableNumber.ToString().Contains(TableNum));
                numberFilters++;
            }
            if(TableCap != null)
            {
                tables = tables.Where(t => t.Capacity.ToString().Contains(TableCap));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(TableLoc)) 
            {
                tables = tables.Where(t => t.Location.ToUpper().Contains(TableLoc.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(StatusFilter))
            {
                tables = tables.Where(t => t.Status == selectedStatus);
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


            return View(await tables.ToListAsync());
        }

        // GET: Table/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .FirstOrDefaultAsync(m => m.ID == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // GET: Table/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Table/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,TableNumber,Capacity,Status,Location")] Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(table);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {

                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Tables.TableNumber"))
                {
                    ModelState.AddModelError("TableNumber", "Unable to save changes. Remember, you cannot have duplicate Table Numbers.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }


            return View(table);
        }

        // GET: Table/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            return View(table);
        }

        // POST: Table/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the table to update
            var tableToUpdate = await _context.Tables.FirstOrDefaultAsync(t => t.ID == id);

            if (tableToUpdate == null)
            {
                return NotFound();
            }
            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Table>(tableToUpdate, "",
                t => t.TableNumber, t => t.Capacity, t => t.Status, t => t.Location))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableExists(tableToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {

                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Tables.TableNumber"))
                    {
                        ModelState.AddModelError("TableNumber", "Unable to save changes. Remember, you cannot have duplicate Table Numbers.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }

            }
            return View(tableToUpdate);
        }

        // GET: Table/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _context.Tables
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }

        // POST: Table/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            try
            {
                if (table != null)
                {
                    _context.Tables.Remove(table);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Table. Remember, you cannot delete a Table that has reservationss assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(table);


        }


        public IActionResult AdminDashboard()
        {
            var totalReservationsToday = _context.Reservations
                .Where(r => r.Date == DateTime.Today).Count();

            var availableTables = _context.Tables
                .Where(t => t.Status == TableStatus.Available).Count();

            var reservationsThisWeek = _context.Reservations
                .Where(r => r.Date >= DateTime.Today && r.Date <= DateTime.Today.AddDays(7)).Count();

            var upcomingReservations = _context.Reservations
                .Where(r => r.Date >= DateTime.Today)
                .OrderBy(r => r.Date)
                .Take(5)
                .ToList();

            var vm = new AdminPageVM
            {
                TotalReservationsToday = totalReservationsToday,
                AvailableTables = availableTables,
                ReservationsThisWeek = reservationsThisWeek,
                UpcomingReservations = upcomingReservations,
                CheckedInReservations = _context.Reservations
                    .Where(r => r.IsCheckedIn).Count(),
                TotalTables = _context.Tables.Count()
            };

            return View(vm);
        }
    

    private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.ID == id);
        }
    }
}
