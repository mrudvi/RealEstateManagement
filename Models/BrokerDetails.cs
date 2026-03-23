using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class BrokerDetails
    {
        [Key] // ✅ make UserId primary key
        public int UserId { get; set; }

        public string? CompanyName { get; set; }

        public string? BrokerLicenseNumber { get; set; }

        public string? LicenseUploadPath { get; set; }

        public string? IdProofPath { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedDate { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}