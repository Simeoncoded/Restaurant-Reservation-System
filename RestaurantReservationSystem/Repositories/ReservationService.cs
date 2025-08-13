using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RestaurantReservationSystem.Data;
using RestaurantReservationSystem.Models;
using System.Drawing.Text;

namespace RestaurantReservationSystem.Repositories
{

    ///<summary>
    ///Production-ready service that talks to your DbContext.
    ///</summary>
    public class ReservationService : IReservationService
    {
        private readonly RestaurantReservationSystemContext _context;

        private static readonly TimeSpan OpenTime = new(10, 0, 0);
        private static readonly TimeSpan CloseTime = new(21, 0, 0);
        private static readonly TimeSpan Slot = TimeSpan.FromMinutes(30);

        public ReservationService(RestaurantReservationSystemContext context)
        {
            _context = context;
        }

        public async Task<List<DateTime>> CheckAvailabilityAsync(DateTime requested, int partySize, CancellationToken ct = default)
        {
            if (partySize <= 0) return new();

            var day = requested.Date;

            // 1) Candidate tables that can seat this party and are available
            var tables = await _context.Tables
                .Where(t => t.Status == TableStatus.Available && t.Capacity >= partySize) // rename Capacity if needed
                .Select(t => new { t.ID, t.Capacity })
                .ToListAsync(ct);

            if (tables.Count == 0) return new();

            // 2) Build 30-min slots from open to close on that day
            var start = day + OpenTime;   // e.g., 10:00
            var end = day + CloseTime;  // e.g., 21:00  

            var allSlots = new List<DateTime>();
            for (var t = start; t <= end; t = t.Add(Slot))
                allSlots.Add(t);

            // 3) Existing reservations that collide with our candidate tables on that day
            var tableIds = tables.Select(t => t.ID).ToList();

            var existing = await _context.Reservations
                .Where(r => r.Date.HasValue && r.Time.HasValue
                            && r.Date.Value.Date == day
                            && tableIds.Contains(r.TableID))
                .Select(r => new { r.TableID, Time = r.Time!.Value })
                .ToListAsync(ct);

            // 4) A slot is available if at least ONE candidate table is free at that time
            var available = new List<DateTime>();

            foreach (var slot in allSlots)
            {
                var slotTime = slot.TimeOfDay;

                // how many of our candidate tables are already taken at this time?
                var takenAtSlot = existing.Count(e => e.Time == slotTime);

                if (takenAtSlot < tables.Count)
                    available.Add(slot);
            }
            return available;
        }


        public async Task<CreateReservationResult> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default)
        {
            //Basic valdations
            if (dto.PartySize <= 0)
            {
                return Fail("Party Size must be greater than 0.");
            }

            //Business hours check
            var day = dto.DateTime.Date;
            var t = dto.DateTime.TimeOfDay;

            if (t < OpenTime || t > CloseTime)
            {
                return Fail($"Requested time {t:hh\\:mm} is outside business hours ({OpenTime:hh\\:mm}–{CloseTime:hh\\:mm}).");
            }

            t = SnapToSlot(t);

            //Load Candidate tables
            List<Table> candidateTables;
            if (dto.TableID > 0)
            {
                var table = await _context.Tables.FirstOrDefaultAsync(x => x.ID == dto.TableID, ct);
                if (table is null)
                {
                    return Fail("Selected table does not exist.");
                }

                if (table.Status != TableStatus.Available)
                {
                    return Fail("Selected table is not available.");
                }

                if (table.Capacity < dto.PartySize)
                {
                    return Fail($"Selected table seats {table.Capacity} which is less than the party size {dto.PartySize}.");
                }

                candidateTables = new() { table };
            }
            else
            {
                candidateTables = await _context.Tables
                                   .Where(x => x.Status == TableStatus.Available && x.Capacity >= dto.PartySize)
                                   .OrderBy(x => x.Capacity)
                                   .ToListAsync(ct);

                if (candidateTables.Count == 0)
                {
                    return Fail("No table can accomodate this party size.");
                }
            }

            //Check conflicts at(Day, time) for each candidate table
            //We want this first table that has no reservation at this slot
            var tableAtSlotTaken = await _context.Reservations
                                    .Where(r => r.Date.HasValue && r.Time.HasValue && r.Date.Value.Date == day && candidateTables.Select(c => c.ID).Contains(r.TableID))
                                    .Select(r => new { r.TableID, r.Time })
                                    .ToListAsync(ct);

            Table? chosen = null;
            foreach (var table in candidateTables)
            {
                var taken = tableAtSlotTaken.Any(e => e.TableID == table.ID && e.Time == t);
                if (!taken) { chosen = table; break; }
            }

            if (chosen is null)
            {
                return Fail("Requested time is fully booked for available tables. Please pick another time.");
            }

            //Create Reservation Entity
            var reservation = new Reservation
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Email = dto.Email,
                Date = day,
                Time = t,
                PartySize = dto.PartySize,
                TableID = chosen.ID,
                Status = ReservationStatus.Pending,
                SpecialRequests = dto.SpecialRequests
            };

            _context.Reservations.Add(reservation);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                // If your DB has a unique index on (Date, Time, TableID), this catches race conditions.
                var msg = ex.GetBaseException().Message;
                if (msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
                    return Fail("A reservation already exists for that table and time. Please try another time.");
                return Fail("Unable to save reservation. Please try again.");
            }

            //simple confirmation
            var conf = $"RSV-{reservation.ID:D6}";
            return new CreateReservationResult
            {
                Success = true,
                ReservationId = reservation.ID,
                ConfirmationNumber = conf
            };
        }



        private static TimeSpan SnapToSlot(TimeSpan t)
        {
            var mins = (int)(Math.Round(t.TotalMinutes / Slot.TotalMinutes) * Slot.TotalMinutes);
            return TimeSpan.FromMinutes(mins);
        }
        private static CreateReservationResult Fail(string msg) => new() { Success = false, Error = msg };

    }
}
