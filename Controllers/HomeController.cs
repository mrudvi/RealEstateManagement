using Microsoft.AspNetCore.Mvc;
using System.Linq;
using RealEstateManagement.Models;


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
                .Where(p => p.PropertyId == id && p.IsVerified)
                .FirstOrDefault();

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }
    }
}