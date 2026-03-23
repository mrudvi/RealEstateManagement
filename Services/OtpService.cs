using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RealEstateManagement.Services
{
    public class OtpService : IOtpService
    {
        private class OtpEntry
        {
            public string? Otp { get; set; }
            public DateTime Expiry { get; set; }
        }

        private static readonly ConcurrentDictionary<int, OtpEntry> _userOtps = new();
        private readonly IEmailService _emailService;

        public OtpService(IEmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task<string> GenerateAndSendOTPAsync(int userId, string email)
        {
            var otp = Random.Shared.Next(100000, 999999).ToString();
            Console.WriteLine("Generated OTP: " + otp);
            var expiry = DateTime.UtcNow.AddMinutes(5);

            _userOtps.AddOrUpdate(userId,
                new OtpEntry { Otp = otp, Expiry = expiry },
                (key, old) => new OtpEntry { Otp = otp, Expiry = expiry });

            await _emailService.SendOTPAsync(email, otp);

            return otp;
        }

        public async Task<bool> VerifyOTPAsync(int userId, string otp)
        {
            await Task.Delay(50); // simulate async work

            if (_userOtps.TryGetValue(userId, out var entry))
            {
                if (entry.Expiry >= DateTime.UtcNow && entry.Otp == otp)
                {
                    _userOtps.TryRemove(userId, out _); // OTP is one-time use
                    return true;
                }
            }

            return false;
        }
    }
}