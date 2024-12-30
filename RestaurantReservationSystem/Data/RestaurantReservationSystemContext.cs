using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;
using System.Numerics;

namespace RestaurantReservationSystem.Data
{
    public class RestaurantReservationSystemContext : DbContext
    {
        public RestaurantReservationSystemContext(DbContextOptions<RestaurantReservationSystemContext> options)
        : base(options)
        {
        }

        public DbSet<Table> Tables { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Prevent Cascade Delete from Table to Reservations
            //so we are prevented from deleting a Table with
            //Reservations assigned
            modelBuilder.Entity<Table>()
                .HasMany<Reservation>(d => d.Reservations)
                .WithOne(p => p.Table)
                .HasForeignKey(p => p.TableID)
                .OnDelete(DeleteBehavior.Restrict);

            //Add a unique index to the Table Number
            modelBuilder.Entity<Table>()
            .HasIndex(p => p.TableNumber)
            .IsUnique();

            //Table Number and Location is unique
            modelBuilder.Entity<Table>()
            .HasIndex(t => new { t.TableNumber, t.Location })
            .IsUnique();

            //Reservation date, time and tableid is unique
            modelBuilder.Entity<Reservation>()
             .HasIndex(r => new { r.Date, r.Time, r.TableID })
            .IsUnique();


        }
    }

}
