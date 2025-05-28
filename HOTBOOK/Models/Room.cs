using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HOTBOOK.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Room type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid room type.")]
        public int? RoomTypeId { get; set; }

        [ForeignKey("RoomTypeId")]
        [ValidateNever]
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
