using System;
using System.Linq;
using System.Threading.Tasks;
using HOTBOOK.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HOTBOOK.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed room types if none exist
            if (!context.RoomTypes.Any())
            {
                var roomTypes = new RoomType[]
                {
                    new RoomType
                    {
                        Name = "Standard Single",
                        Capacity = 1,
                        Description = "Comfortable single room with a queen-size bed, perfect for solo travelers."
                    },
                    new RoomType
                    {
                        Name = "Standard Double",
                        Capacity = 2,
                        Description = "Spacious double room with two queen-size beds, ideal for couples or friends."
                    },
                    new RoomType
                    {
                        Name = "Deluxe King",
                        Capacity = 2,
                        Description = "Luxurious room with a king-size bed and premium amenities."
                    },
                    new RoomType
                    {
                        Name = "Family Suite",
                        Capacity = 4,
                        Description = "Large suite with two bedrooms, perfect for families."
                    },
                    new RoomType
                    {
                        Name = "Executive Suite",
                        Capacity = 2,
                        Description = "Premium suite with separate living area and business amenities."
                    },
                    new RoomType
                    {
                        Name = "Presidential Suite",
                        Capacity = 4,
                        Description = "Our most luxurious accommodation with multiple rooms and premium services."
                    },
                    new RoomType
                    {
                        Name = "Accessible Room",
                        Capacity = 2,
                        Description = "Specially designed room for guests with mobility needs."
                    },
                    new RoomType
                    {
                        Name = "Junior Suite",
                        Capacity = 2,
                        Description = "Comfortable suite with a separate sitting area."
                    },
                    new RoomType
                    {
                        Name = "Garden View Room",
                        Capacity = 2,
                        Description = "Peaceful room overlooking our beautiful gardens."
                    },
                    new RoomType
                    {
                        Name = "City View Room",
                        Capacity = 2,
                        Description = "Modern room with stunning city views."
                    }
                };

                await context.RoomTypes.AddRangeAsync(roomTypes);
                await context.SaveChangesAsync();
            }

            // Seed rooms if none exist
            if (!context.Rooms.Any())
            {
                var roomTypes = await context.RoomTypes.ToListAsync();
                var random = new Random();
                var rooms = new List<Room>();

                // Create 50 rooms
                for (int i = 1; i <= 50; i++)
                {
                    var roomType = roomTypes[random.Next(roomTypes.Count)];
                    var floor = (i - 1) / 10 + 1; // 5 floors, 10 rooms each
                    var roomNumber = $"{floor}{(i % 10 == 0 ? 10 : i % 10):D2}";

                    // Base price varies by room type
                    var basePrice = roomType.Name switch
                    {
                        "Standard Single" => 100m,
                        "Standard Double" => 150m,
                        "Deluxe King" => 200m,
                        "Family Suite" => 300m,
                        "Executive Suite" => 400m,
                        "Presidential Suite" => 800m,
                        "Accessible Room" => 150m,
                        "Junior Suite" => 250m,
                        "Garden View Room" => 180m,
                        "City View Room" => 180m,
                        _ => 150m
                    };

                    // Add some random variation to the price
                    var priceVariation = random.Next(-20, 21);
                    var finalPrice = basePrice + priceVariation;

                    rooms.Add(new Room
                    {
                        RoomNumber = roomNumber,
                        RoomTypeId = roomType.Id,
                        PricePerNight = finalPrice,
                        IsAvailable = true,
                        Floor = floor,
                        Status = "Available",
                        Description = $"{roomType.Name} on floor {floor}. {roomType.Description}"
                    });
                }

                await context.Rooms.AddRangeAsync(rooms);
                await context.SaveChangesAsync();
            }
        }
    }
} 