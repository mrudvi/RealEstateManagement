using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class EnquiryResponse
    {
        [Key]
        public int ResponseId { get; set; }

        [Required]
        public int EnquiryId { get; set; }

        [Required]
        public int RespondedBy { get; set; }

        [Required(ErrorMessage = "Response Message is required")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Response must be between 5 and 2000 characters")]
        public string? ResponseMessage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("EnquiryId")]
        public virtual Enquiry? Enquiry { get; set; }

        [ForeignKey("RespondedBy")]
        public virtual User? RespondedByUser { get; set; }
        public virtual User? RespondedByNavigation { get; set; }
    }
}