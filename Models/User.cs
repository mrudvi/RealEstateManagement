using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }
        [NotMapped]
        public IFormFile? LicenseFile { get; set; }

        [NotMapped]
        public IFormFile? IdProofFile { get; set; }

        // inside User class

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 100 characters")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last Name must be between 2 and 100 characters")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
        [StringLength(15)]
        public required string PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        [NotMapped]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Street Address is required")]
        [StringLength(255)]
        public required string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public required string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(50)]
        public required string State { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50)]
        public required string Country { get; set; }

        [NotMapped] // Not stored directly in DB; you can store path instead
        [Required(ErrorMessage = "Profile Image is required")]
        public IFormFile? ProfileImageFile { get; set; }


        [StringLength(255)]
        public string? ProfileImage { get; set; } //path saved in db

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [NotMapped]
        public string? BrokerLicenseNumber { get; set; }

        [NotMapped]
        public string? CompanyName { get; set; }

        [NotMapped]
        public string? ReraNumber { get; set; }
        [NotMapped]
        public IFormFile? CompanyDocumentsFile { get; set; }
        // Navigation properties
        [InverseProperty("Owner")]
        public virtual ICollection<Property> PropertyOwners { get; set; } = new List<Property>();

        [InverseProperty("Broker")]
        public virtual ICollection<Property> BrokedProperties { get; set; } = new List<Property>();

        // public ICollection<Enquiry> Enquiries { get; set; } = new List<Enquiry>();
        public ICollection<EnquiryResponse> EnquiryResponses { get; set; } = new List<EnquiryResponse>();
        public ICollection<SiteVisit> SiteVisits { get; set; } = new List<SiteVisit>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        // public ICollection<Enquiry> Enquiry { get; set; } = new List<Enquiry>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Enquiry> EnquiryRespondedByNavigations { get; set; } = new List<Enquiry>();
        public virtual ICollection<Enquiry> EnquiryCustomers { get; set; } = new List<Enquiry>();
        public virtual ICollection<SiteVisit> SitevisitCustomers { get; set; } = new List<SiteVisit>();
        public virtual ICollection<Property> PropertyBudders { get; set; } = new List<Property>();
        public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();
        [InverseProperty("ApprovedByNavigation")]

        public virtual ICollection<Property> PropertyApprovedByNavigations { get; set; } = new List<Property>();

        public virtual ICollection<SiteVisit> SitevisitScheduledByNavigations { get; set; } = new List<SiteVisit>();
        public virtual ICollection<Transaction> TransactionApprovedByNavigations { get; set; } = new List<Transaction>();

        public virtual ICollection<Transaction> TransactionBuyers { get; set; } = new List<Transaction>();

        public virtual ICollection<Transaction> TransactionSellers { get; set; } = new List<Transaction>();

        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserType
    {
        Customer,
        PropertyOwner,
        Broker,
        Builder,
        Admin
    }
}