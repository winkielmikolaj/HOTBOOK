using System;
using System.ComponentModel.DataAnnotations;

namespace HOTBOOK.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; }

        [Required]
        public int RoomTypeId { get; set; }
        public RoomType RoomType { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public string Description { get; set; }

        [Required]
        public int Floor { get; set; }

        public string Status { get; set; } // Available, Occupied, Maintenance
    }
} 