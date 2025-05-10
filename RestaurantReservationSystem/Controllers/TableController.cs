using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestaurantReservationSystem.CustomControllers;
using RestaurantReservationSystem.Data;
using RestaurantReservationSystem.Models;
using RestaurantReservationSystem.Utilities;
using RestaurantReservationSystem.ViewModels;

namespace RestaurantReservationSystem.Controllers
{
    public class TableController : ElephantController
    {
        private readonly RestaurantReservationSystemContext _context;

        public TableController(RestaurantReservationSystemContext context)
        {
            _context = context;
        }

        // GET: Table
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string? TableNum, string? TableCap, string? TableLoc, string? StatusFilter,
            string? actionButton, string sortDirection = "asc", string sortField = "Table")
        {
            //List of sort options.
            
            string[] sortOptions = new[] { "Table", "Capacity", "Status", "Location" };

            //Count the number of filters applied 
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

            //Now we know which field and direction to sort by
            if (sortField == "Capacity")
            {
                if (sortDirection == "asc")
                {
                    tables = tables
                        .OrderByDescending(t => t.Capacity);
                }
                else
                {
                    tables = tables
                        .OrderBy(t => t.Capacity);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    tables = tables
                        .OrderByDescending(t => t.Status);
                }
                else
                {
                    tables = tables
                        .OrderBy(t => t.Status);
                }
            }
            else if (sortField == "Location")
            {
                if (sortDirection == "asc")
                {
                    tables = tables
                        .OrderByDescending(t => t.Location);
                }
                else
                {
                    tables = tables
                        .OrderBy(t => t.Location);
                }
            }

            else 
            {
                if (sortDirection == "asc")
                {
                    tables = tables
                        .OrderBy(t => t.TableNumber);
                }
                else
                {
                    tables = tables
                        .OrderByDescending(t => t.TableNumber);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Table>.CreateAsync(tables.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
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
                    return RedirectToAction("Details", new { table.ID });

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

            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.ID == id);

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
                    return RedirectToAction("Details", new { tableToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("","Concurrency Error");
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
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);

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
        var today = DateTime.Today;

        var totalReservationsToday = _context.Reservations
            .Where(r => r.Date == today)
            .Count();

        var availableTables = _context.Tables
            .Where(t => t.Status == TableStatus.Available)
            .Count();

        var reservationsThisWeek = _context.Reservations
            .Where(r => r.Date >= today && r.Date <= today.AddDays(7))
            .Count();

        var upcomingReservations = _context.Reservations
            .Where(r => r.Date >= today)
            .OrderBy(r => r.Date)
            .Take(5)
            .ToList();

        var checkedInReservations = _context.Reservations
            .Where(r => r.IsCheckedIn)
            .Count();

        var totalTables = _context.Tables.Count();

        // 👇 Chart data: Reservations grouped by table name
        var reservationByTable = _context.Reservations
            .Include(r => r.Table)
            .GroupBy(r => r.Table.TableNumber) // or use r.TableId if no name
            .Select(group => new
            {
                table = group.Key,
                count = group.Count()
            })
            .ToList();

        // Pass to view as JSON for Chart.js
        ViewData["data"] = JsonConvert.SerializeObject(reservationByTable);

        var vm = new AdminPageVM
        {
            TotalReservationsToday = totalReservationsToday,
            AvailableTables = availableTables,
            ReservationsThisWeek = reservationsThisWeek,
            UpcomingReservations = upcomingReservations,
            CheckedInReservations = checkedInReservations,
            TotalTables = totalTables
        };

        return View(vm);
    }




    private bool TableExists(int id)
        {
            return _context.Tables.Any(e => e.ID == id);
        }
    }
}
