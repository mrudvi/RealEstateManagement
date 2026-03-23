using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class Propertyimage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Image Path is required")]
        [StringLength(255)]
        public required string ImagePath { get; set; }

        public bool IsPrimary { get; set; } = false;

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PropertyId")]
        public required Property Property { get; set; }
    }
}