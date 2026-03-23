using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateManagement.Models
{
    public class Propertytype
    {
        [Key]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Type Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Type Name must be between 2 and 50 characters")]
        public string? TypeName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}