using Microsoft.AspNetCore.Mvc;
using RealEstateManagement.Models;
using RealEstateManagement.Services;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace RealEstateManagement.Controllers
{
    public class PropertyOwnerController : Controller
    {
        private readonly RealestatemanagementContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public PropertyOwnerController(
            RealestatemanagementContext context,
            IWebHostEnvironment hostingEnvironment,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        private int GetUserId()
        {
            var userIdInt = HttpContext.Session.GetInt32("UserId");
            if (!userIdInt.HasValue)
            {
                return 0;
            }
            return userIdInt.Value;
        }

        private bool CheckUserAuthentication()
        {
            int userId = GetUserId();

            if (userId == 0)
            {
                return false;
            }

            bool isPropertyOwner = _context.UserRoles
                .Any(ur => ur.UserId == userId && ur.Role.RoleName == "PropertyOwner");

            return isPropertyOwner;
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
    .Any(ur => ur.UserId == userId && ur.Role.RoleName == "PropertyOwner");

            if (user == null || !isBroker)
            {
                return RedirectToAction("Index", "Home");
            }
            var properties = _context.Properties
                .Where(p => p.OwnerId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var totalEnquiries = _context.Enquiries
    .Where(e => e.Property != null && e.Property.OwnerId == userId)
    .Count();

            var totalVisits = _context.Sitevisits
                .Where(sv => sv.Property != null && sv.Property.OwnerId == userId)
                .Count();

            var activeProperties = properties.Where(p => p.Status == PropertyStatus.Active && p.IsVerified).Count();
            var pendingProperties = properties.Where(p => !p.IsVerified).Count();

            ViewBag.TotalProperties = properties.Count;
            ViewBag.ActiveProperties = activeProperties;
            ViewBag.PendingProperties = pendingProperties;
            ViewBag.TotalEnquiries = totalEnquiries;
            ViewBag.TotalVisits = totalVisits;
            ViewBag.UserName = user.FullName;

            return View(properties);
        }

        public IActionResult AddProperty()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
            ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();

            return View(new Property());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProperty(Property model, IFormFile mainImage)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(model);
            }

            int userId = GetUserId();
            model.OwnerId = userId;
            model.Status = PropertyStatus.Pending;
            model.IsVerified = false;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;

            // Validate image
            if (mainImage == null || mainImage.Length == 0)
            {
                ModelState.AddModelError("MainImage", "Please select a property image");
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(model);
            }

            if (mainImage.Length > 5 * 1024 * 1024) // 5MB
            {
                ModelState.AddModelError("MainImage", "Image size must be less than 5MB");
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(model);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(mainImage.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("MainImage", "Only image files are allowed (jpg, png, gif)");
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(model);
            }

            // Handle main image upload
            try
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "properties");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    mainImage.CopyTo(fileStream);
                }

                model.MainImage = Path.Combine("uploads", "properties", fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("MainImage", "Error uploading image: " + ex.Message);
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(model);
            }

            _context.Properties.Add(model);
            _context.SaveChanges();

            LogActivity(userId, "Property Added", "Property", model.PropertyId, $"Property '{model.PropertyTitle}' added");

            TempData["SuccessMessage"] = "Property added successfully! Admin approval is pending.";
            return RedirectToAction("Index");
        }

        public IActionResult EditProperty(int id)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null)
            {
                TempData["ErrorMessage"] = "Property not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
            ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProperty(int id, Property model, IFormFile mainImage)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null)
            {
                TempData["ErrorMessage"] = "Property not found";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(property);
            }

            try
            {
                property.PropertyTitle = model.PropertyTitle;
                property.PropertyDescription = model.PropertyDescription;
                property.PropertyTypeId = model.PropertyTypeId;
                property.LocalityId = model.LocalityId;
                property.PropertyPrice = model.PropertyPrice;
                property.PropertyArea = model.PropertyArea;
                property.AreaUnit = model.AreaUnit;
                property.Bedrooms = model.Bedrooms;
                property.Bathrooms = model.Bathrooms;
                property.Parking = model.Parking;
                property.TransactionType = model.TransactionType;
                property.Address = model.Address;
                property.LandmarkDescription = model.LandmarkDescription;
                property.AgeOfProperty = model.AgeOfProperty;
                property.Furnishing = model.Furnishing;
                property.Facing = model.Facing;
                property.Features = model.Features;
                property.UpdatedDate = DateTime.Now;

                // Handle image update
                if (mainImage != null && mainImage.Length > 0)
                {
                    // Validate image
                    if (mainImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("MainImage", "Image size must be less than 5MB");
                        ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                        ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                        return View(property);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(mainImage.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("MainImage", "Only image files are allowed");
                        ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                        ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                        return View(property);
                    }

                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "properties");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        mainImage.CopyTo(fileStream);
                    }

                    property.MainImage = Path.Combine("uploads", "properties", fileName).Replace("\\", "/");
                }

                _context.Properties.Update(property);
                _context.SaveChanges();

                LogActivity(userId, "Property Updated", "Property", property.PropertyId, $"Property '{property.PropertyTitle}' updated");

                TempData["SuccessMessage"] = "Property updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating property: " + ex.Message;
                ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
                ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
                return View(property);
            }
        }

        [HttpPost]
        public IActionResult DeleteProperty(int id)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            try
            {
                _context.Properties.Remove(property);
                _context.SaveChanges();

                LogActivity(userId, "Property Deleted", "Property", id, $"Property '{property.PropertyTitle}' deleted");

                return Json(new { success = true, message = "Property deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting property: " + ex.Message });
            }
        }

        public IActionResult PropertyEnquiries(int id)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null)
            {
                TempData["ErrorMessage"] = "Property not found";
                return RedirectToAction("Index");
            }

            var enquiries = _context.Enquiries
                .Where(e => e.PropertyId == id)
                .OrderByDescending(e => e.CreatedDate)
                .ToList();

            ViewBag.Property = property;
            ViewBag.TotalEnquiries = enquiries.Count;
            ViewBag.NewEnquiries = enquiries.Where(e => e.EnquiryStatus == EnquiryStatus.New).Count();

            return View(enquiries);
        }

        [HttpPost]
        public IActionResult RespondToEnquiry(int enquiryId, string response)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (string.IsNullOrWhiteSpace(response))
            {
                return Json(new { success = false, message = "Response message is required" });
            }

            if (response.Length < 5)
            {
                return Json(new { success = false, message = "Response must be at least 5 characters" });
            }

            if (response.Length > 2000)
            {
                return Json(new { success = false, message = "Response cannot exceed 2000 characters" });
            }

            int userId = GetUserId();
            var enquiry = _context.Enquiries.FirstOrDefault(e => e.EnquiryId == enquiryId);

            if (enquiry == null)
            {
                return Json(new { success = false, message = "Enquiry not found" });
            }

            // Ensure property is not null
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == enquiry.PropertyId && p.OwnerId == userId);
            if (property == null)
            {
                return Json(new { success = false, message = "Unauthorized or property not found" });
            }

            try
            {
                var enquiryResponse = new EnquiryResponse
                {
                    EnquiryId = enquiryId,
                    RespondedBy = userId,
                    ResponseMessage = response,
                    CreatedDate = DateTime.Now
                };

                enquiry.EnquiryStatus = EnquiryStatus.Contacted;
                enquiry.RespondedBy = userId;
                enquiry.RespondedDate = DateTime.Now;
                enquiry.UpdatedDate = DateTime.Now;

                _context.Enquiryresponses.Add(enquiryResponse);
                _context.Enquiries.Update(enquiry);
                _context.SaveChanges();

                // Send email to customer safely
                var customer = _context.Users.FirstOrDefault(u => u.UserId == enquiry.CustomerId);
                if (customer != null)
                {
                    var propertyTitle = property.PropertyTitle ?? "Unknown Property";

                    _emailService.SendEnquiryResponseAsync(customer.Email, propertyTitle, response).Wait();

                    _notificationService.CreateNotificationAsync(
                        enquiry.CustomerId,
                        "Enquiry Response",
                        $"You received a response for your enquiry on {property.PropertyTitle ?? "Unknown Property"}",
                        "Email"
                    ).Wait();
                }

                LogActivity(userId, "Enquiry Responded", "Enquiry", enquiryId, "Response sent to enquiry");

                return Json(new { success = true, message = "Response sent successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public IActionResult PropertySiteVisits(int id)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null)
            {
                TempData["ErrorMessage"] = "Property not found";
                return RedirectToAction("Index");
            }

            var visits = _context.Sitevisits
                .Where(sv => sv.PropertyId == id)
                .OrderByDescending(sv => sv.ScheduledDate)
                .ToList();

            var upcomingVisits = visits.Where(v => v.ScheduledDate >= DateTime.Now && v.VisitStatus == VisitStatus.Scheduled).Count();
            var completedVisits = visits.Where(v => v.VisitStatus == VisitStatus.Completed).Count();

            ViewBag.Property = property;
            ViewBag.TotalVisits = visits.Count;
            ViewBag.UpcomingVisits = upcomingVisits;
            ViewBag.CompletedVisits = completedVisits;

            return View(visits);
        }

        [HttpPost]
        public IActionResult UpdateVisitStatus(int visitId, string status)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            int userId = GetUserId();
            var visit = _context.Sitevisits.FirstOrDefault(sv => sv.VisitId == visitId);

            if (visit == null)
            {
                return Json(new { success = false, message = "Visit not found" });
            }

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == visit.PropertyId && p.OwnerId == userId);

            if (property == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (Enum.TryParse<VisitStatus>(status, out var visitStatus))
            {
                try
                {
                    visit.VisitStatus = visitStatus;
                    visit.UpdatedDate = DateTime.Now;
                    _context.Sitevisits.Update(visit);
                    _context.SaveChanges();

                    LogActivity(userId, "Visit Status Updated", "SiteVisit", visitId, $"Visit status changed to {status}");

                    return Json(new { success = true, message = "Visit status updated successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid status" });
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
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                TempData["ErrorMessage"] = "User not found";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", user);
            }

            try
            {
                // Validate phone number
                if (!model.PhoneNumber.All(char.IsDigit) || model.PhoneNumber.Length != 10)
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number must be exactly 10 digits");
                    return View("Profile", user);
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.State = model.State;
                user.Country = model.Country;
                user.UpdatedDate = DateTime.Now;

                _context.Users.Update(user);
                _context.SaveChanges();

                // Update session
                HttpContext.Session.SetString("UserName", user.FullName);

                LogActivity(userId, "Profile Updated", "User", userId, "User profile updated");

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating profile: " + ex.Message;
                return View("Profile", user);
            }
        }

        [HttpPost]
        public IActionResult UpdateProfilePicture(IFormFile profileImage)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (profileImage == null || profileImage.Length == 0)
            {
                return Json(new { success = false, message = "Please select an image" });
            }

            if (profileImage.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Image size must be less than 5MB" });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profileImage.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Only image files are allowed" });
            }

            try
            {
                int userId = GetUserId();
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{userId}_{DateTime.Now.Ticks}{extension}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(fileStream);
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.ProfileImage = Path.Combine("uploads", "profiles", fileName).Replace("\\", "/");
                    user.UpdatedDate = DateTime.Now;
                    _context.Users.Update(user);
                    _context.SaveChanges();

                    LogActivity(userId, "Profile Picture Updated", "User", userId, "Profile picture uploaded");

                    return Json(new { success = true, message = "Profile picture updated successfully", imageUrl = user.ProfileImage });
                }

                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public IActionResult Settings()
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
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Json(new { success = false, message = "All fields are required" });
            }

            if (newPassword.Length < 6)
            {
                return Json(new { success = false, message = "New password must be at least 6 characters" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Passwords do not match" });
            }

            int userId = GetUserId();
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            try
            {
                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedDate = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();

                LogActivity(userId, "Password Changed", "User", userId, "User changed password");

                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private void LogActivity(int userId, string action, string entityType, int entityId, string description)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging activity: {ex.Message}");
            }
        }
    }
}