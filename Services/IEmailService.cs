using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateManagement.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> SendOTPAsync(string email, string otp);
        Task<bool> SendEnquiryConfirmationAsync(string email, string propertyTitle, string customerName);
        Task<bool> SendEnquiryResponseAsync(string email, string propertyTitle, string message);
        Task<bool> SendSiteVisitConfirmationAsync(string email, string propertyTitle, string visitDate, string visitTime);
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);
    }
}