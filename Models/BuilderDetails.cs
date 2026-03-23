using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RealEstateManagement.Models;

public class BuilderDetails
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Company Name is required")]
    public string CompanyName { get; set; } = null!; // important
    public string? CompanyDocumentsPath { get; set; }
    public string? ReraNumber { get; set; }
    public bool IsVerified { get; set; } = false;

    public DateTime CreatedDate { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}