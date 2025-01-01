using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;

namespace RestaurantReservationSystem.Data
{
    public class ReservationRepository
    {
        private readonly RestaurantReservationSystemContext _context;

        public ReservationRepository(RestaurantReservationSystemContext context)
        {
            _context = context;
        }

        // Update the table status to unavailable
        public async Task<bool> UpdateTableStatusAsync(int tableId)
        {
            var table = await _context.Tables.FirstOrDefaultAsync(t => t.ID == tableId);

            if (table == null || table.Status == TableStatus.Unavailable)
            {
                return false;
            }

            table.Status = TableStatus.Unavailable;
            _context.Update(table);
            await _context.SaveChangesAsync();

            return true;
        }

        // Create a new reservation
        public async Task CreateReservationAsync(Reservation reservation)
        {
            _context.Add(reservation);
            await _context.SaveChangesAsync();
        }
    }

}
