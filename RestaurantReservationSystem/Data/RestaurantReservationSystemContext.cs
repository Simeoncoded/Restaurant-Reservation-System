using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;
using System.Numerics;

namespace RestaurantReservationSystem.Data
{
    public class RestaurantReservationSystemContext : DbContext
    {

        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }


        public RestaurantReservationSystemContext(DbContextOptions<RestaurantReservationSystemContext> options, IHttpContextAccessor httpContextAccessor)
             : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }

        public RestaurantReservationSystemContext(DbContextOptions<RestaurantReservationSystemContext> options)
            : base(options)
        {
            _httpContextAccessor = null!;
            UserName = "Seed Data";
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

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }

}
