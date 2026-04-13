using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagement.Models;
using System.Linq;

namespace RealEstateManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly RealestatemanagementContext _context;

        public AdminController(RealestatemanagementContext context)
        {
            _context = context;
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
            int userId = GetUserId();

            if (userId == 0)
                return false;

            bool isAdmin = _context.UserRoles
                .Any(ur => ur.UserId == userId && ur.Role.RoleName == "Admin");

            return isAdmin;
        }

        public IActionResult ViewUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            // Get Broker Details
            var broker = _context.BrokerDetails.FirstOrDefault(b => b.UserId == id);

            // Get Builder Details
            var builder = _context.BuilderDetails.FirstOrDefault(b => b.UserId == id);

            ViewBag.Broker = broker;
            ViewBag.Builder = builder;

            return View(user);
        }
        public IActionResult PendingUsers()
        {
            var pendingUsers = _context.Users
                .Where(u => u.UserRoles.Any(r => r.RoleId == 3 || r.RoleId == 4))
                .Select(u => new
                {
                    User = u,

                    Type = u.UserRoles.Any(r => r.RoleId == 3) ? "Broker" : "Builder",

                    // 🔥 FETCH FROM BrokerDetails TABLE
                    License = _context.BrokerDetails
                        .Where(b => b.UserId == u.UserId)
                        .Select(b => b.LicenseUploadPath)
                        .FirstOrDefault(),

                    IdProof = _context.BrokerDetails
                        .Where(b => b.UserId == u.UserId)
                        .Select(b => b.IdProofPath)
                        .FirstOrDefault(),

                    // 🔥 FETCH FROM BuilderDetails TABLE
                    CompanyDoc = _context.BuilderDetails
                    .Where(b => b.UserId == u.UserId)
                    .Select(b => b.CompanyDocumentsPath) // ✅ correct
                    .FirstOrDefault()
                })
                .ToList();

            return View(pendingUsers);
        }
        public IActionResult ApproveUser(int id)
        {
            // Get role
            var role = _context.UserRoles
                .Where(r => r.UserId == id)
                .Select(r => r.Role.RoleName)
                .FirstOrDefault();

            if (role == "Broker")
            {
                var broker = _context.BrokerDetails.FirstOrDefault(b => b.UserId == id);
                if (broker != null)
                {
                    broker.IsVerified = true;
                }
            }
            else if (role == "Builder")
            {
                var builder = _context.BuilderDetails.FirstOrDefault(b => b.UserId == id);
                if (builder != null)
                {
                    builder.IsVerified = true;
                }
            }

            _context.SaveChanges();

            return RedirectToAction("PendingUsers");
        }

        public IActionResult RejectUser(int id)
        {
            // Reject Broker
            var broker = _context.BrokerDetails.FirstOrDefault(b => b.UserId == id);
            if (broker != null)
            {
                broker.IsVerified = false;
            }

            // Reject Builder
            var builder = _context.BuilderDetails.FirstOrDefault(b => b.UserId == id);
            if (builder != null)
            {
                builder.IsVerified = false;
            }

            _context.SaveChanges();

            return RedirectToAction("PendingUsers");
        }

        public IActionResult Index()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var totalUsers = _context.Users.Count(u => u.IsActive);
            var totalProperties = _context.Properties.Count();
            var pendingProperties = _context.Properties.Count(p => p.Status == PropertyStatus.Pending && !p.IsVerified);
            var totalEnquiries = _context.Enquiries.Count();
            var activeProperties = _context.Properties.Count(p => p.Status == PropertyStatus.Active && p.IsVerified);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProperties = totalProperties;
            ViewBag.PendingProperties = pendingProperties;
            ViewBag.TotalEnquiries = totalEnquiries;
            ViewBag.ActiveProperties = activeProperties;

            var recentActivities = _context.ActivityLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToList();

            return View(recentActivities);
        }

        public IActionResult ManageUsers(string userType = "")
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(userType))
            {
                if (!string.IsNullOrEmpty(userType))
                {
                    users = users.Where(u => u.UserRoles.Any(r => r.Role.RoleName == userType));
                }
            }

            var userList = users.OrderByDescending(u => u.CreatedDate).ToList();
            ViewBag.UserTypes = Enum.GetNames(typeof(UserType));

            return View(userList);
        }

        [HttpPost]
        public IActionResult ToggleUserStatus(int userId)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null || user.UserId == GetUserId())
            {
                return Json(new { success = false, message = "User not found or cannot modify own account" });
            }

            user.IsActive = !user.IsActive;
            user.UpdatedDate = DateTime.Now;

            _context.Users.Update(user);
            _context.SaveChanges();

            LogActivity(GetUserId(), "User Status Changed", "User", userId, $"User {user.FullName} status changed to {(user.IsActive ? "Active" : "Inactive")}");

            return Json(new { success = true, message = "User status updated successfully" });
        }

        public IActionResult ManageProperties()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var properties = _context.Properties
         .Include(p => p.PropertyType)
         .Include(p => p.Owner)
         .Include(p => p.Locality)
         .OrderByDescending(p => p.CreatedDate)
         .ToList();

            return View(properties);
        }

        public IActionResult ApproveProperty(int id)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        public IActionResult ApprovePropertyConfirm(int propertyId)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            int adminId = GetUserId();
            property.IsVerified = true;
            property.Status = PropertyStatus.Active;
            property.ApprovedBy = adminId;
            property.ApprovalDate = DateTime.Now;
            property.UpdatedDate = DateTime.Now;

            _context.Properties.Update(property);
            _context.SaveChanges();

            LogActivity(adminId, "Property Approved", "Property", propertyId, $"Property '{property.PropertyTitle}' approved");

            return Json(new { success = true, message = "Property approved successfully" });
        }

        [HttpPost]
        public IActionResult RejectProperty(int propertyId, string reason)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Rejection reason is required" });
            }

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            int adminId = GetUserId();
            property.Status = PropertyStatus.Inactive;
            property.RejectionReason = reason;
            property.UpdatedDate = DateTime.Now;

            _context.Properties.Update(property);
            _context.SaveChanges();

            LogActivity(adminId, "Property Rejected", "Property", propertyId, $"Property '{property.PropertyTitle}' rejected: {reason}");

            return Json(new { success = true, message = "Property rejected successfully" });
        }

        [HttpPost]
        public IActionResult TogglePropertyStatus(int propertyId)
        {
            if (!CheckUserAuthentication())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var property = _context.Properties.FirstOrDefault(p => p.PropertyId == propertyId);

            if (property == null)
            {
                return Json(new { success = false, message = "Property not found" });
            }

            if (property.Status == PropertyStatus.Active)
            {
                property.Status = PropertyStatus.Inactive;
            }
            else
            {
                property.Status = PropertyStatus.Active;
            }

            property.UpdatedDate = DateTime.Now;

            _context.Properties.Update(property);
            _context.SaveChanges();

            LogActivity(GetUserId(), "Property Status Changed", "Property", propertyId,
                $"Property status changed to {property.Status}");

            return Json(new { success = true, message = "Property status updated successfully" });
        }

        public IActionResult ManageLocalities()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var localities = _context.Localities
                .OrderByDescending(l => l.CreatedDate)
                .ToList();

            return View(localities);
        }

        public IActionResult AddLocality()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(new Locality());
        }

        [HttpPost]
        public IActionResult AddLocality(Locality model)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Localities.Any(l => l.LocalityName == model.LocalityName))
            {
                ModelState.AddModelError("LocalityName", "Locality already exists");
                return View(model);
            }

            model.CreatedDate = DateTime.Now;

            _context.Localities.Add(model);
            _context.SaveChanges();

            LogActivity(GetUserId(), "Locality Added", "Locality", model.LocalityId, $"Locality '{model.LocalityName}' added");

            TempData["SuccessMessage"] = "Locality added successfully";
            return RedirectToAction("ManageLocalities");
        }

        public IActionResult EditLocality(int id)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var locality = _context.Localities.FirstOrDefault(l => l.LocalityId == id);

            if (locality == null)
            {
                return NotFound();
            }

            return View(locality);
        }

        [HttpPost]
        public IActionResult EditLocality(Locality model)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var locality = _context.Localities.FirstOrDefault(l => l.LocalityId == model.LocalityId);

            if (locality == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Localities.Any(l => l.LocalityName == model.LocalityName && l.LocalityId != model.LocalityId))
            {
                ModelState.AddModelError("LocalityName", "Locality name already exists");
                return View(model);
            }

            locality.LocalityName = model.LocalityName;
            locality.Area = model.Area;
            locality.ZipCode = model.ZipCode;
            locality.Description = model.Description;
            locality.IsActive = model.IsActive;

            _context.Localities.Update(locality);
            _context.SaveChanges();

            LogActivity(GetUserId(), "Locality Updated", "Locality", locality.LocalityId, $"Locality '{locality.LocalityName}' updated");

            TempData["SuccessMessage"] = "Locality updated successfully";
            return RedirectToAction("ManageLocalities");
        }

        public IActionResult ManagePropertyTypes()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var propertyTypes = _context.Propertytypes
                .OrderByDescending(pt => pt.CreatedDate)
                .ToList();

            return View(propertyTypes);
        }

        public IActionResult AddPropertyType()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(new Propertytype());
        }

        [HttpPost]
        public IActionResult AddPropertyType(Propertytype model)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Propertytypes.Any(pt => pt.TypeName == model.TypeName))
            {
                ModelState.AddModelError("TypeName", "Property Type already exists");
                return View(model);
            }

            model.CreatedDate = DateTime.Now;

            _context.Propertytypes.Add(model);
            _context.SaveChanges();

            LogActivity(GetUserId(), "Property Type Added", "PropertyType", model.PropertyTypeId, $"Property Type '{model.TypeName}' added");

            TempData["SuccessMessage"] = "Property Type added successfully";
            return RedirectToAction("ManagePropertyTypes");
        }

        public IActionResult EditPropertyType(int id)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var propertyType = _context.Propertytypes.FirstOrDefault(pt => pt.PropertyTypeId == id);

            if (propertyType == null)
            {
                return NotFound();
            }

            return View(propertyType);
        }

        [HttpPost]
        public IActionResult EditPropertyType(Propertytype model)
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var propertyType = _context.Propertytypes.FirstOrDefault(pt => pt.PropertyTypeId == model.PropertyTypeId);

            if (propertyType == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Propertytypes.Any(pt => pt.TypeName == model.TypeName && pt.PropertyTypeId != model.PropertyTypeId))
            {
                ModelState.AddModelError("TypeName", "Property Type name already exists");
                return View(model);
            }

            propertyType.TypeName = model.TypeName;
            propertyType.Description = model.Description;
            propertyType.IsActive = model.IsActive;

            _context.Propertytypes.Update(propertyType);
            _context.SaveChanges();

            LogActivity(GetUserId(), "Property Type Updated", "PropertyType", propertyType.PropertyTypeId, $"Property Type '{propertyType.TypeName}' updated");

            TempData["SuccessMessage"] = "Property Type updated successfully";
            return RedirectToAction("ManagePropertyTypes");
        }

        public IActionResult ActivityLog()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            var logs = _context.ActivityLogs
                .OrderByDescending(a => a.CreatedDate)
                .Take(100)
                .ToList();

            return View(logs);
        }

        public IActionResult SystemReports()
        {
            if (!CheckUserAuthentication())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalProperties = _context.Properties.Count();
            ViewBag.TotalEnquiries = _context.Enquiries.Count();
            ViewBag.TotalVisits = _context.Sitevisits.Count();

            ViewBag.UsersByType = _context.UserRoles
            .GroupBy(ur => ur.Role.RoleName)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToList();

            ViewBag.PropertiesByStatus = _context.Properties
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToList();

            ViewBag.PropertiesByLocality = _context.Properties
    .Where(p => p.Locality != null)
    .GroupBy(p => p.Locality!.LocalityName)
    .Select(g => new { Locality = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .Take(10)
    .ToList();

            return View();
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