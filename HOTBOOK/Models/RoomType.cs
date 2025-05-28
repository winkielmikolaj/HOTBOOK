using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HOTBOOK.Models
{
    public class RoomType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public int Capacity { get; set; }

        public string Description { get; set; }

        public ICollection<Room> Rooms { get; set; }
    }
} 