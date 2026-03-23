using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateManagement.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOTPAsync(int userId, string email);
        Task<bool> VerifyOTPAsync(int userId, string otp);
    }
}