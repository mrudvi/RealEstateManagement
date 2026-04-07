using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Property Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Property Title must be between 5 and 200 characters")]
        public string? PropertyTitle { get; set; }

        [Required(ErrorMessage = "Property Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Property Description must be between 10 and 2000 characters")]
        public string? PropertyDescription { get; set; }

        [Required(ErrorMessage = "Property Type is required")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Locality is required")]
        public int LocalityId { get; set; }

        [Required(ErrorMessage = "Owner is required")]
        public int OwnerId { get; set; }

        public int? BrokerId { get; set; }

        public int? BudderId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 999999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal PropertyPrice { get; set; }

        [Required(ErrorMessage = "Property Area is required")]
        [Range(1, 999999.99, ErrorMessage = "Area must be greater than 0")]
        public decimal PropertyArea { get; set; }

        [Required]
        public AreaUnit AreaUnit { get; set; } = AreaUnit.SqFt;

        [Range(0, 20, ErrorMessage = "Bedrooms must be between 0 and 20")]
        public int Bedrooms { get; set; } = 0;

        [Range(0, 20, ErrorMessage = "Bathrooms must be between 0 and 20")]
        public int Bathrooms { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "Parking must be between 0 and 10")]
        public int Parking { get; set; } = 0;

        [Required(ErrorMessage = "Transaction Type is required")]
        public TransactionType TransactionType { get; set; }

        public PropertyStatus Status { get; set; } = PropertyStatus.Pending;

        public int? ApprovedBy { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 255 characters")]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? LandmarkDescription { get; set; }

        [Range(0, 200, ErrorMessage = "Age of Property must be between 0 and 200 years")]
        public int? AgeOfProperty { get; set; }

        public Furnishing Furnishing { get; set; } = Furnishing.Unfurnished;

        public Facing Facing { get; set; } = Facing.North;

        [StringLength(1000)]
        public string? Features { get; set; }

        [StringLength(255)]
        public string? MainImage { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime? ApprovalDate { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PropertyTypeId")]
        public virtual Propertytype PropertyType { get; set; } = null!;

        [ForeignKey("LocalityId")]
        public virtual Locality Locality { get; set; } = null!;

        [ForeignKey("OwnerId")]
        public virtual User? Owner { get; set; }

        [ForeignKey("BrokerId")]
        public virtual User? Broker { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByNavigation { get; set; }

        [ForeignKey("BudderId")]
        public virtual User? Budder { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public ICollection<Propertyimage> PropertyImages { get; set; } = new List<Propertyimage>();
        public ICollection<Enquiry> Enquiry { get; set; } = new List<Enquiry>();
        public ICollection<SiteVisit> SiteVisits { get; set; } = new List<SiteVisit>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public enum PropertyStatus
    {
        Pending,
        Active,
        Inactive,
        Sold,
        Rented
    }

    public enum TransactionType
    {
        Buy,
        Sell,
        Rent
    }

    public enum AreaUnit
    {
        SqFt,
        SqMeter
    }

    public enum Furnishing
    {
        Furnished,
        SemiFurnished,
        Unfurnished
    }

    public enum Facing
    {
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest
    }
}