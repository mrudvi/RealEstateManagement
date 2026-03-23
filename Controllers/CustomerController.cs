using Microsoft.AspNetCore.Mvc;
using RealEstateManagement.Models;
using RealEstateManagement.Services;
using System.Linq;

namespace RealEstateManagement.Controllers
{
    public class CustomerController : Controller
    {
        private readonly RealestatemanagementContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public CustomerController(
               RealestatemanagementContext context,
               IEmailService emailService,
               INotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        private int GetUserId()
        {
            var userIdStr = HttpContext.Session.GetInt32("UserId");
            if (!userIdStr.HasValue)
            {
                return 0;
            }
            return userIdStr.Value;
        }

        private bool CheckUserAuthentication()
        {
            if (GetUserId() == 0)
            {
                return false;
            }
            return true;
        }

        public IActionResult Index()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            bool isBroker = _context.UserRoles
    .Any(ur => ur.UserId == userId && ur.Role.RoleName == "Customer");

            if (user == null || !isBroker)
            {
                return RedirectToAction("Index", "Home");
            }

            var properties = _context.Properties
                .Where(p => p.Status == Models.PropertyStatus.Active && p.IsVerified)
                .OrderByDescending(p => p.CreatedDate)
                .Take(10)
                .ToList();

            var enquiries = _context.Enquiries
                .Where(e => e.CustomerId == userId)
                .OrderByDescending(e => e.CreatedDate)
                .Take(5)
                .ToList();

            var favorites = _context.Favorites
                .Where(f => f.CustomerId == userId)
                .Count();

            ViewBag.TotalEnquiries = enquiries.Count;
            ViewBag.TotalFavorites = favorites;

            return View(properties);
        }

        public IActionResult MyEnquiries()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var enquiries = _context.Enquiries
                .Where(e => e.CustomerId == userId)
                .OrderByDescending(e => e.CreatedDate)
                .ToList();

            return View(enquiries);
        }

        public IActionResult MyFavorites()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var favorites = _context.Favorites
                .Where(f => f.CustomerId == userId)
                .ToList();

            return View(favorites);
        }

        [HttpPost]
        public IActionResult AddFavorite(int propertyId)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            var existingFavorite = _context.Favorites
                .FirstOrDefault(f => f.CustomerId == userId && f.PropertyId == propertyId);

            if (existingFavorite != null)
            {
                return Json(new { success = false, message = "Already in favorites" });
            }

            var favorite = new Favorite
            {
                CustomerId = userId,
                PropertyId = propertyId,
                AddedDate = DateTime.Now
            };

            _context.Favorites.Add(favorite);
            _context.SaveChanges();

            return Json(new { success = true, message = "Added to favorites" });
        }

        [HttpPost]
        public IActionResult RemoveFavorite(int propertyId)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            int userId = GetUserId();
            var favorite = _context.Favorites
                .FirstOrDefault(f => f.CustomerId == userId && f.PropertyId == propertyId);

            if (favorite == null)
            {
                return Json(new { success = false, message = "Favorite not found" });
            }

            _context.Favorites.Remove(favorite);
            _context.SaveChanges();

            return Json(new { success = true, message = "Removed from favorites" });
        }

        [HttpPost]
        public IActionResult SubmitEnquiry(int propertyId, string message)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (string.IsNullOrWhiteSpace(message) || message.Length < 5 || message.Length > 2000)
            {
                return Json(new { success = false, message = "Message must be between 5 and 2000 characters" });
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            var enquiry = new Enquiry
            {
                PropertyId = propertyId,
                CustomerId = userId,
                EnquiryMessage = message,
                EnquiryStatus = EnquiryStatus.New,
                Priority = Priority.Medium,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Enquiries.Add(enquiry);
            _context.SaveChanges();

            // Get current user for email
            var currentUser = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            // Send confirmation to customer
            _emailService.SendEnquiryConfirmationAsync(currentUser.Email, property.PropertyTitle!, currentUser.FullName).Wait();

            // Send notification to property owner
            _notificationService.SendEnquiryNotificationAsync(property.OwnerId, property.PropertyTitle!, currentUser.FullName, currentUser.Email).Wait();

            LogActivity(userId, "Enquiry Submitted", "Enquiry", enquiry.EnquiryId, $"Enquiry submitted for property {property.PropertyTitle}");

            return Json(new { success = true, message = "Enquiry submitted successfully" });
        }

        [HttpPost]
        public IActionResult ScheduleSiteVisit(int propertyId, string visitDate, string visitTime)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (!DateTime.TryParse(visitDate, out DateTime scheduledDate))
            {
                return Json(new { success = false, message = "Invalid date format" });
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            var siteVisit = new SiteVisit
            {
                PropertyId = propertyId,
                CustomerId = userId,
                ScheduledDate = scheduledDate,
                ScheduledTime = visitTime,
                VisitStatus = VisitStatus.Scheduled,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Sitevisits.Add(siteVisit);
            _context.SaveChanges();

            LogActivity(userId, "Site Visit Scheduled", "SiteVisit", siteVisit.VisitId, $"Site visit scheduled for {property.PropertyTitle}");

            return Json(new { success = true, message = "Site visit scheduled successfully" });
        }

        public IActionResult MySiteVisits()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var visits = _context.Sitevisits
                .Where(sv => sv.CustomerId == userId)
                .OrderByDescending(sv => sv.ScheduledDate)
                .ToList();

            return View(visits);
        }

        public IActionResult Profile()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateProfile(User model)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", user);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.UpdatedDate = DateTime.Now;

            _context.Users.Update(user);
            _context.SaveChanges();

            LogActivity(userId, "Profile Updated", "User", userId, "User profile updated");

            TempData["SuccessMessage"] = "Profile updated successfully";
            return RedirectToAction("Profile");
        }

        private void LogActivity(int userId, string action, string entityType, int entityId, string description)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedDate = DateTime.Now
            };
            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }
    }
}