using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateManagement.Services
{
    public interface INotificationService
    {
        Task<bool> CreateNotificationAsync(int userId, string subject, string message, string type);
        Task<bool> SendEnquiryNotificationAsync(int propertyOwnerId, string propertyTitle, string customerName, string customerEmail);
        Task<bool> SendVisitNotificationAsync(int propertyOwnerId, string propertyTitle, string customerName, string visitDate);
    }
}