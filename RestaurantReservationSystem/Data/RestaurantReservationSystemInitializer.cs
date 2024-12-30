using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Models;
using System.Diagnostics;

namespace RestaurantReservationSystem.Data
{
    public class RestaurantReservationSystemInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new RestaurantReservationSystemContext(
                serviceProvider.GetRequiredService<DbContextOptions<RestaurantReservationSystemContext>>()))
            {
                try
                {
                    // Seed Tables (5 different tables)
                    if (!context.Tables.Any())
                    {
                        context.Tables.AddRange(
                            new Table
                            {
                                TableNumber = 1,
                                Capacity = 4,
                                Status = TableStatus.Available,
                                Location = "Indoor"
                            },
                            new Table
                            {
                                TableNumber = 2,
                                Capacity = 2,
                                Status = TableStatus.Available,
                                Location = "Outdoor"
                            },
                            new Table
                            {
                                TableNumber = 3,
                                Capacity = 6,
                                Status = TableStatus.Available,
                                Location = "Indoor"
                            },
                            new Table
                            {
                                TableNumber = 4,
                                Capacity = 8,
                                Status = TableStatus.Available,
                                Location = "Private Room"
                            },
                            new Table
                            {
                                TableNumber = 5,
                                Capacity = 2,
                                Status = TableStatus.Available,
                                Location = "Outdoor"
                            });
                        context.SaveChanges();
                    }

                    // Seed Reservations (5 different reservations)
                    if (!context.Reservations.Any())
                    {
                        context.Reservations.AddRange(
                            new Reservation
                            {
                                FirstName = "Fred",
                                LastName = "Flintstone",
                                Phone = "9055551212",
                                Email = "fflintstone@outlook.com",
                                Date = DateTime.Parse("2025-01-11"),
                                Time = new TimeSpan(18, 0, 0), // 6:00 PM
                                PartySize = 4,
                                Status = ReservationStatus.Confirmed,
                                SpecialRequests = "Vegetarian meal",
                                IsCheckedIn = false,
                                TableID = 1 // Table 1
                            },
                            new Reservation
                            {
                                FirstName = "Wilma",
                                LastName = "Flintstone",
                                Phone = "9055551213",
                                Email = "wflintstone@outlook.com",
                                Date = DateTime.Parse("2025-01-11"),
                                Time = new TimeSpan(19, 30, 0), // 7:30 PM
                                PartySize = 2,
                                Status = ReservationStatus.Confirmed,
                                SpecialRequests = "Window seat",
                                IsCheckedIn = false,
                                TableID = 2 // Table 2
                            },
                            new Reservation
                            {
                                FirstName = "Barney",
                                LastName = "Rubble",
                                Phone = "9055551214",
                                Email = "brubble@outlook.com",
                                Date = DateTime.Parse("2025-01-12"),
                                Time = new TimeSpan(20, 0, 0), // 8:00 PM
                                PartySize = 6,
                                Status = ReservationStatus.Confirmed,
                                SpecialRequests = "Birthday celebration",
                                IsCheckedIn = false,
                                TableID = 3 // Table 3
                            },
                            new Reservation
                            {
                                FirstName = "Dino",
                                LastName = "Rubble",
                                Phone = "9055551215",
                                Email = "dinorubble@outlook.com",
                                Date = DateTime.Parse("2025-01-12"),
                                Time = new TimeSpan(18, 30, 0), // 6:30 PM
                                PartySize = 8,
                                Status = ReservationStatus.Confirmed,
                                SpecialRequests = "Private room, Anniversary",
                                IsCheckedIn = false,
                                TableID = 4 // Table 4
                            },
                            new Reservation
                            {
                                FirstName = "Pebbles",
                                LastName = "Flintstone",
                                Phone = "9055551216",
                                Email = "pebbles@outlook.com",
                                Date = DateTime.Parse("2025-01-13"),
                                Time = new TimeSpan(17, 0, 0), // 5:00 PM
                                PartySize = 2,
                                Status = ReservationStatus.Confirmed,
                                SpecialRequests = "Near the bar",
                                IsCheckedIn = false,
                                TableID = 5 // Table 5
                            });
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
            }
        }
    }
}
