using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using RealEstateManagement.Models;

namespace RealEstateManagement.Services
{
    public class EmailService : IEmailService
    {

        private readonly EmailConfiguration _emailConfig;

        public EmailService(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
        }

        // ===== Helper method inside the class =====
        private MailAddress GetFromAddress()
        {
            if (string.IsNullOrWhiteSpace(_emailConfig.SenderEmail))
                throw new InvalidOperationException("SenderEmail cannot be null or empty.");

            return new MailAddress(_emailConfig.SenderEmail!, _emailConfig.SenderName ?? "");
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_emailConfig.SenderEmail!, _emailConfig.SenderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailConfig.SenderEmail, _emailConfig.SenderPassword),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);   // VERY IMPORTANT
                return false;
            }
        }

        public Task<bool> SendOTPAsync(string email, string otp)
        {
            string subject = "Your Real Estate Surat - OTP Verification";
            string body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                            <h2>Real Estate Surat</h2>
                        </div>
                        <div style='padding: 20px; background-color: #f5f1e8;'>
                            <p>Hello,</p>
                            <p>Your One-Time Password (OTP) for email verification is:</p>
                            <div style='background-color: white; padding: 15px; text-align: center; border: 2px solid #D4A574; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='color: #D4A574; font-size: 24px; letter-spacing: 5px;'>{otp}</h3>
                            </div>
                            <p>This OTP is valid for 10 minutes only.</p>
                            <p>If you didn't request this OTP, please ignore this email.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                        </div>
                    </body>
                </html>";
            return SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEnquiryConfirmationAsync(string email, string propertyTitle, string customerName)
        {
            string subject = "Enquiry Received - Real Estate Surat";
            string body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                            <h2>Real Estate Surat</h2>
                        </div>
                        <div style='padding: 20px; background-color: #f5f1e8;'>
                            <p>Dear {customerName},</p>
                            <p>Thank you for your interest in our property:</p>
                            <div style='background-color: white; padding: 15px; border-left: 4px solid #D4A574; margin: 20px 0;'>
                                <h4 style='color: #B8956D;'>{propertyTitle}</h4>
                                <p>Your enquiry has been successfully submitted. The property owner will contact you soon with more details.</p>
                            </div>
                            <p>We appreciate your interest and will ensure a smooth process.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                        </div>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEnquiryResponseAsync(string email, string propertyTitle, string message)
        {
            string subject = "Response to Your Enquiry - Real Estate Surat";
            string body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                            <h2>Real Estate Surat</h2>
                        </div>
                        <div style='padding: 20px; background-color: #f5f1e8;'>
                            <p>Hello,</p>
                            <p>You have received a response for your enquiry about:</p>
                            <div style='background-color: white; padding: 15px; border-left: 4px solid #D4A574; margin: 20px 0;'>
                                <h4 style='color: #B8956D;'>{propertyTitle}</h4>
                                <p><strong>Message:</strong></p>
                                <p>{message}</p>
                            </div>
                            <p>Please log in to your account for more details.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                        </div>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendSiteVisitConfirmationAsync(string email, string propertyTitle, string visitDate, string visitTime)
        {
            string subject = "Site Visit Scheduled - Real Estate Surat";
            string body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                            <h2>Real Estate Surat</h2>
                        </div>
                        <div style='padding: 20px; background-color: #f5f1e8;'>
                            <p>Hello,</p>
                            <p>Your site visit has been successfully scheduled for:</p>
                            <div style='background-color: white; padding: 15px; border: 2px solid #D4A574; border-radius: 5px; margin: 20px 0;'>
                                <h4 style='color: #B8956D;'>{propertyTitle}</h4>
                                <p><strong>Date:</strong> {visitDate}</p>
                                <p><strong>Time:</strong> {visitTime}</p>
                            </div>
                            <p>Please arrive 10 minutes before the scheduled time. If you need to reschedule, please log in to your account.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                        </div>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            string subject = "Welcome to Real Estate Surat";
            string body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #D4A574; color: white; padding: 20px; text-align: center;'>
                            <h2>Real Estate Surat</h2>
                        </div>
                        <div style='padding: 20px; background-color: #f5f1e8;'>
                            <p>Welcome {firstName},</p>
                            <p>Thank you for registering with Real Estate Surat! We're excited to help you find the perfect property.</p>
                            <div style='background-color: white; padding: 15px; border-left: 4px solid #D4A574; margin: 20px 0;'>
                                <h4>Get Started:</h4>
                                <ul>
                                    <li>Complete your profile</li>
                                    <li>Search for properties by location and price</li>
                                    <li>Save your favorite properties</li>
                                    <li>Submit enquiries directly to property owners</li>
                                </ul>
                            </div>
                            <p>If you have any questions, feel free to contact us.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>© 2026 Real Estate Surat. All rights reserved.</p>
                        </div>
                    </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }
    }
}