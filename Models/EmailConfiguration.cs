using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateManagement.Models
{
    public class EmailConfiguration
    {
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderPassword { get; set; }
        public bool EnableSSL { get; set; }
        public string SenderName { get; set; } = "Real Estate Surat";

    }
}