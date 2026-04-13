using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class SiteVisit
    {
        [Key]
        public int VisitId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Scheduled Date is required")]
        public DateTime ScheduledDate { get; set; }

        public TimeSpan ScheduledTime { get; set; }

        public VisitStatus VisitStatus { get; set; } = VisitStatus.Scheduled;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public int? ScheduledBy { get; set; }

        // Navigation properties
        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }

        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }

        [ForeignKey("ScheduledBy")]
        public virtual User? ScheduledByUser { get; set; }
    }

    public enum VisitStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        NoShow
    }
}