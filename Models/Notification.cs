using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagement.Models;

public partial class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(255)]
    public string? Subject { get; set; }

    [Required]
    [StringLength(1000)]
    public string? Message { get; set; }

    [StringLength(50)]
    public string? NotificationType { get; set; } // 'Email', 'SMS', 'In-App'

    public bool IsRead { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? ReadDate { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}
