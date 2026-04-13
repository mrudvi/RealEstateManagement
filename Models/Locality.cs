using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateManagement.Models
{
    public class Locality
    {
        [Key]
        public int LocalityId { get; set; }

        [Required(ErrorMessage = "Locality Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Locality Name must be between 2 and 100 characters")]
        public string? LocalityName { get; set; }

        [StringLength(100)]
        public string? Area { get; set; }

        [StringLength(10)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Zip Code must be 6 digits")]
        public string? ZipCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}