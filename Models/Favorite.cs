using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }


        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }
    }
}