using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HOTBOOK.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [ValidateNever]
        public ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [ValidateNever]
        public Room Room { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        [Display(Name = "Check-in Date")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Total price is required")]
        [Display(Name = "Total Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        [Required(ErrorMessage = "Booking date is required")]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; }

        [Required(ErrorMessage = "Number of guests is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }
    }
} 