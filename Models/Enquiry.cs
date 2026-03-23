using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class Enquiry
    {
        [Key]
        public int EnquiryId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Message must be between 5 and 2000 characters")]
        public required string EnquiryMessage { get; set; }

        public EnquiryStatus EnquiryStatus { get; set; } = EnquiryStatus.New;

        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public DateTime? RespondedDate { get; set; }
        public int? RespondedBy { get; set; }

        // Navigation properties
        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }


        [ForeignKey("CustomerId")]
        public virtual User? Customer { get; set; }


        [ForeignKey("RespondedBy")]
        public virtual User? RespondedByUser { get; set; }
        public ICollection<EnquiryResponse> Responses { get; set; } = new List<EnquiryResponse>();
    }

    public enum EnquiryStatus
    {
        New,
        InProgress,
        Contacted,
        Closed,
        Rejected
    }

    public enum Priority
    {
        Low,
        Medium,
        High
    }
}