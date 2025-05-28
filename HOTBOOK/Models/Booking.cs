using System;
using System.ComponentModel.DataAnnotations;

namespace HOTBOOK.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int GuestId { get; set; }
        public Guest Guest { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room Room { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } // Pending, Confirmed, Cancelled, Completed

        public DateTime BookingDate { get; set; }

        public string SpecialRequests { get; set; }

        public int NumberOfGuests { get; set; }
    }
} 