using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RealEstateManagement.Models;
using Microsoft.EntityFrameworkCore;


namespace RealEstateManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly RealestatemanagementContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(RealestatemanagementContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            // Pass user login status to view
            ViewBag.IsLoggedIn = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId") != null;

            // Example for stats
            ViewBag.TotalProperties = _context.Properties.Count(p => p.Status == Models.PropertyStatus.Active);
            ViewBag.TotalUsers = _context.Users.Count();

            var featuredProperties = _context.Properties
             .Include(p => p.Locality)
             .Include(p => p.PropertyType)
             .Include(p => p.Owner)     // ✅ ADD THIS
             .Include(p => p.Broker)    // ✅ ADD THIS
             .Where(p => p.Status == Models.PropertyStatus.Active)
             .Take(6)
             .ToList();

            return View(featuredProperties);
        }

        public IActionResult Search(string locality, string propertyType, decimal? minPrice, decimal? maxPrice, string transactionType)
        {
            var query = _context.Properties
                .Where(p => p.Status == Models.PropertyStatus.Active && p.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrEmpty(locality))
            {
                query = query.Where(p => p.Locality.LocalityName != null && p.Locality.LocalityName.Contains(locality));
            }

            if (!string.IsNullOrEmpty(propertyType))
            {
                query = query.Where(p => p.PropertyType.TypeName == propertyType);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PropertyPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PropertyPrice <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(p => p.TransactionType.ToString() == transactionType);
            }

            var properties = query.OrderByDescending(p => p.CreatedDate).ToList();

            ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
            ViewBag.PropertyTypes = _context.Propertytypes.Where(pt => pt.IsActive).ToList();

            return View(properties);
        }

        public IActionResult PropertyDetails(int id)
        {
            var property = _context.Properties
         .Include(p => p.Locality)
         .Include(p => p.Owner)     // 🔥 ADD THIS
         .Include(p => p.Broker)    // 🔥 ADD THIS
         .FirstOrDefault(p => p.PropertyId == id && p.IsVerified);

            if (property == null)
            {
                return NotFound();
            }

            // ✅ LOAD MULTIPLE IMAGES
            var images = _context.Propertyimages
                .Where(x => x.PropertyId == id)
                .ToList();

            // ✅ LOAD VIDEO
            var video = _context.PropertyVideos
                .FirstOrDefault(v => v.PropertyId == id);

            // ✅ PASS TO VIEW
            ViewBag.Images = images;
            ViewBag.Video = video;

            return View(property);
        }

        [HttpPost]
        public IActionResult BookVisit([FromBody] SiteVisit model)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new { success = false, message = "Please login first" });
                }

                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                // ✅ Fix time parsing issue
                if (model.ScheduledTime == TimeSpan.Zero)
                {
                    return Json(new { success = false, message = "Invalid time" });
                }

                model.CustomerId = userId.Value;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;
                model.VisitStatus = VisitStatus.Scheduled;

                var property = _context.Properties
                    .FirstOrDefault(p => p.PropertyId == model.PropertyId);
                if (property == null)
                {
                    return Json(new { success = false, message = "Property not found" });
                }
                if (property != null)
                {
                    if (property.BrokerId != null)
                        model.ScheduledBy = property.BrokerId;
                    else if (property.BudderId != null)
                        model.ScheduledBy = property.BudderId;
                    else
                        model.ScheduledBy = property.OwnerId;
                }

                _context.Sitevisits.Add(model);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}