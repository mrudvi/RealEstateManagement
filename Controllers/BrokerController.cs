using Microsoft.AspNetCore.Mvc;
using RealEstateManagement.Models;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace RealEstateManagement.Controllers
{
    public class BrokerController : Controller
    {
        private readonly RealestatemanagementContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BrokerController(RealestatemanagementContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
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
        [HttpGet]
        public async Task<IActionResult> SearchLocation(string query)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "RealEstateApp");

                var url = $"https://nominatim.openstreetmap.org/search?format=json&q={query}+surat&limit=5";

                var response = await client.GetStringAsync(url);

                return Content(response, "application/json");
            }
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
.Any(ur => ur.UserId == userId && ur.Role.RoleName == "Broker");

            if (user == null || !isBroker)
            {
                return RedirectToAction("Index", "Home");
            }

            var properties = _context.Properties
                .Where(p => p.BrokerId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var totalEnquiries = _context.Enquiries
    .Where(e => e.Property != null && e.Property.BrokerId == userId)
    .Count();

            var totalVisits = _context.Sitevisits
    .Where(sv => sv.Property != null && sv.Property.BrokerId == userId)
    .Count();

            ViewBag.TotalProperties = properties.Count;
            ViewBag.TotalEnquiries = totalEnquiries;
            ViewBag.TotalVisits = totalVisits;

            return View(properties);
        }

        public IActionResult AddProperty()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewBag.Owners = _context.Users.Where(u => u.IsActive).ToList(); // 🔥 ADD THIS
            ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
            ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();

            return View(new Property());
        }

        [HttpPost]
        public IActionResult AddProperty(Property model, IFormFile mainImage)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            // if (!ModelState.IsValid)
            // {
            //     ModelState.Clear();   // 🔥 ADD THIS LINE

            //     ViewBag.Owners = _context.Users.Where(u => u.IsActive).ToList();
            //     ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();
            //     ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
            //     return View(model);
            // }

            int userId = GetUserId();
            model.BrokerId = userId;
            model.Status = PropertyStatus.Pending;
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;

            //one image path inside properties table
            if (mainImage != null && mainImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "properties");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    mainImage.CopyTo(fileStream);
                }

                model.MainImage = Path.Combine("uploads", "properties", fileName).Replace("\\", "/");
            }

            _context.Properties.Add(model);
            _context.SaveChanges();

            // SAVE IMAGES into propertyimage table
            var images = Request.Form.Files.Where(f => f.Name == "images");

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "images");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    var propertyImage = new Propertyimage
                    {
                        PropertyId = model.PropertyId,
                        ImagePath = "uploads/images/" + fileName,
                        IsPrimary = false
                    };

                    _context.Propertyimages.Add(propertyImage);
                }
            }
            // ✅ SAVE VIDEO into propertyvideo table
            var video = Request.Form.Files.FirstOrDefault(f => f.Name == "video");

            if (video != null && video.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    video.CopyTo(stream);
                }

                var propertyVideo = new Propertyvideo
                {
                    PropertyId = model.PropertyId,
                    VideoPath = "uploads/videos/" + fileName
                };

                _context.PropertyVideos.Add(propertyVideo);
            }

            LogActivity(userId, "Property Added", "Property", model.PropertyId, $"Property '{model.PropertyTitle}' added by broker");

            TempData["SuccessMessage"] = "Property added successfully and pending approval";
            return RedirectToAction("Index");
        }

        public IActionResult ManageEnquiries()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();
            var enquiries = _context.Enquiries
    .Where(e => e.Property != null && e.Property.BrokerId == userId)
    .OrderByDescending(e => e.CreatedDate)
    .ToList();

            return View(enquiries);
        }

        [HttpPost]
        public IActionResult RespondToEnquiry(int enquiryId, string response)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Please login first" });
            }

            if (string.IsNullOrWhiteSpace(response) || response.Length < 5 || response.Length > 2000)
            {
                return Json(new { success = false, message = "Response must be between 5 and 2000 characters" });
            }

            int userId = GetUserId();
            var enquiry = _context.Enquiries.FirstOrDefault(e => e.EnquiryId == enquiryId);

            if (enquiry == null)
            {
                return Json(new { success = false, message = "Enquiry not found" });
            }

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == enquiry.PropertyId && p.BrokerId == userId);

            if (property == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

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

            LogActivity(userId, "Enquiry Responded", "Enquiry", enquiryId, "Response sent to enquiry");

            return Json(new { success = true, message = "Response sent successfully" });
        }

        public IActionResult ManageSiteVisits()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            int userId = GetUserId();

            var visits = _context.Sitevisits
                .Include(sv => sv.Property)   // optional but useful
                .Include(sv => sv.Customer)   // who booked
                .Where(sv => sv.ScheduledBy == userId)  // ✅ FIXED
                .OrderByDescending(sv => sv.ScheduledDate)
                .ToList();

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

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == visit.PropertyId && p.BrokerId == userId);

            if (property == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (Enum.TryParse<VisitStatus>(status, out var visitStatus))
            {
                visit.VisitStatus = visitStatus;
                visit.UpdatedDate = DateTime.Now;
                _context.Sitevisits.Update(visit);
                _context.SaveChanges();

                LogActivity(userId, "Visit Status Updated", "SiteVisit", visitId, $"Visit status changed to {status}");

                return Json(new { success = true, message = "Visit status updated successfully" });
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

            LogActivity(userId, "Profile Updated", "User", userId, "Broker profile updated");

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