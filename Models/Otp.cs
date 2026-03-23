using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models;

public partial class Otp
{
    public int Otpid { get; set; }

    public int UserId { get; set; }

    public string Otpcode { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool? IsVerified { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    [ForeignKey("UserId")]

    public virtual User User { get; set; } = null!;
}
