using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public int? BuyerId { get; set; }

        public int? SellerId { get; set; }

        [Required(ErrorMessage = "Transaction Date is required")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 999999999.99, ErrorMessage = "Amount must be greater than 0")]
        public decimal TransactionAmount { get; set; }

        public TransactionStatus TransactionStatus { get; set; } = TransactionStatus.Pending;

        [StringLength(100)]
        public required string PaymentMethod { get; set; }

        [StringLength(255)]
        public required string DocumentPath { get; set; }

        public int? ApprovedBy { get; set; }

        [StringLength(500)]
        public required string Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PropertyId")]
        public required Property Property { get; set; }

        [ForeignKey("BuyerId")]
        public required User Buyer { get; set; }

        [ForeignKey("SellerId")]
        public required User Seller { get; set; }

        [ForeignKey("ApprovedBy")]
        public required User ApprovedByUser { get; set; }

        public virtual User? ApprovedByNavigation { get; set; }
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}