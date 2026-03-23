using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Action is required")]
        [StringLength(255)]
        public required string Action { get; set; }

        [StringLength(50)]
        public required string EntityType { get; set; }

        public int? EntityId { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        [StringLength(45)]
        public required string? IPAddress { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}