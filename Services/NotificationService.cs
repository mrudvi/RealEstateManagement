using System;
using System.Threading.Tasks;
using RealEstateManagement.Models;

namespace RealEstateManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly RealestatemanagementContext _context;
        private readonly IEmailService _emailService;

        public NotificationService(RealestatemanagementContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> CreateNotificationAsync(int userId, string subject, string message, string type)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Subject = subject,
                    Message = message,
                    NotificationType = type,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notification creation error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEnquiryNotificationAsync(int propertyOwnerId, string propertyTitle, string customerName, string customerEmail)
        {
            try
            {
                var owner = _context.Users.FirstOrDefault(u => u.UserId == propertyOwnerId);

                if (owner == null)
                    return false;

                // Create in-app notification
                await CreateNotificationAsync(
                    propertyOwnerId,
                    "New Enquiry Received",
                    $"{customerName} has submitted an enquiry for {propertyTitle}",
                    "In-App"
                );

                // Send email notification
                string subject = "New Enquiry Received - Real Estate Surat";
                string body = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                                <h2>Real Estate Surat</h2>
                            </div>
                            <div style='padding: 20px; background-color: #f5f1e8;'>
                                <p>Hello {owner.FirstName},</p>
                                <p>You have received a new enquiry for your property:</p>
                                <div style='background-color: white; padding: 15px; border-left: 4px solid #D4A574; margin: 20px 0;'>
                                    <h4 style='color: #B8956D;'>{propertyTitle}</h4>
                                    <p><strong>From:</strong> {customerName}</p>
                                    <p><strong>Email:</strong> {customerEmail}</p>
                                </div>
                                <p>Please log in to your dashboard to respond to the enquiry.</p>
                                <hr>
                                <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                            </div>
                        </body>
                    </html>";

                await _emailService.SendEmailAsync(owner.Email, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Enquiry notification error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendVisitNotificationAsync(int propertyOwnerId, string propertyTitle, string customerName, string visitDate)
        {
            try
            {
                var owner = _context.Users.FirstOrDefault(u => u.UserId == propertyOwnerId);

                if (owner == null)
                    return false;

                // Create in-app notification
                await CreateNotificationAsync(
                    propertyOwnerId,
                    "New Site Visit Scheduled",
                    $"{customerName} has scheduled a visit for {propertyTitle} on {visitDate}",
                    "In-App"
                );

                // Send email notification
                string subject = "Site Visit Scheduled - Real Estate Surat";
                string body = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                                <h2>Real Estate Surat</h2>
                            </div>
                            <div style='padding: 20px; background-color: #f5f1e8;'>
                                <p>Hello {owner.FirstName},</p>
                                <p>A customer has scheduled a site visit for your property:</p>
                                <div style='background-color: white; padding: 15px; border-left: 4px solid #D4A574; margin: 20px 0;'>
                                    <h4 style='color: #B8956D;'>{propertyTitle}</h4>
                                    <p><strong>Customer:</strong> {customerName}</p>
                                    <p><strong>Scheduled Date:</strong> {visitDate}</p>
                                </div>
                                <p>Please log in to your dashboard to view more details.</p>
                                <hr>
                                <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                            </div>
                        </body>
                    </html>";

                await _emailService.SendEmailAsync(owner.Email, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Visit notification error: {ex.Message}");
                return false;
            }
        }
    }
}