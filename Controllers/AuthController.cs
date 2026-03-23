using Microsoft.AspNetCore.Mvc;
using RealEstateManagement.Models;
using RealEstateManagement.Services;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly RealestatemanagementContext _context;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public AuthController(RealestatemanagementContext context, IOtpService otpService, IEmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendOTPAsync("jariwalamrudvi@gmail.com", "123456");
            return Content("Test email sent");
        }
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }
        // public async Task<IActionResult> Register(User model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return View(model);
        //     }

        //     if (_context.Users.Any(u => u.Email == model.Email))
        //     {
        //         ModelState.AddModelError("Email", "Email already registered");
        //         return View(model);
        //     }

        //     // Hash password
        //     model.PasswordHash = HashPassword(model.Password);

        //     model.City = "Surat";
        //     model.ProfileImage = "default.jpg";
        //     model.CreatedDate = DateTime.Now;
        //     model.UpdatedDate = DateTime.Now;
        //     model.IsVerified = false;

        //     _context.Users.Add(model);
        //     var userRole = new UserRole
        //     {
        //         UserId = model.UserId,
        //         RoleId = 1
        //     };

        //     _context.UserRoles.Add(userRole);
        //     await _context.SaveChangesAsync();
        //     await _emailService.SendOTPAsync("jariwalamrudvi@gmail.com", "123456");
        //     await _emailService.SendWelcomeEmailAsync(model.Email, model.FirstName);
        //     await _otpService.GenerateAndSendOTPAsync(model.UserId, model.Email);

        //     LogActivity(model.UserId, "User Registered", "User", model.UserId, $"{model.FullName} registered");
        //     TempData["SuccessMessage"] = "Registration successful! Please verify your email.";
        //     return RedirectToAction("VerifyOTP", new { email = model.Email });
        // }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            // 1. Validate model
            if (!ModelState.IsValid)
                return View(model);

            // 2. Check email
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered");
                return View(model);
            }

            // 3. Check image
            if (model.ProfileImageFile == null)
            {
                ModelState.AddModelError("ProfileImageFile", "Profile Image is required");
                return View(model);
            }

            // 4. Save Image
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImageFile.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImageFile.CopyToAsync(stream);
            }


            // 5. Create User
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Country = model.Country,
                PasswordHash = HashPassword(model.Password),
                ProfileImage = "/images/profiles/" + fileName,
                IsActive = true,
                IsVerified = false,   // OTP pending
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // 6. Get selected role (IMPORTANT FIX)
            // var role = _context.Roles.FirstOrDefault(r => r.RoleId == model.RoleId);

            int finalRoleId;

            // 👉 Handle Customer / Owner case
            if (model.RoleId.ToString() == "customer")
            {
                finalRoleId = _context.Roles
                    .First(r => r.RoleName!.ToLower() == "customer").RoleId;
            }
            else
            {
                if (model.RoleId <= 0)
                {
                    ModelState.AddModelError("RoleId", "Please select a role");
                    return View(model);
                }

                finalRoleId = model.RoleId;
            }

            // 👉 Now get role safely
            var role = _context.Roles.Find(finalRoleId);

            if (role == null)
            {
                ModelState.AddModelError("RoleId", "Invalid role selected");
                return View(model);
            }

            Console.WriteLine("Final RoleId: " + finalRoleId);

            // 7. Save user FIRST
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 8. Assign role (ONLY ONCE ✅)
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId
            });

            // 9. Broker logic
            if (role.RoleName!.ToLower() == "broker")
            {
                string? licensePath = null;
                string? idProofPath = null;

                // 📁 Save License File
                if (model.LicenseFile != null)
                {
                    var licenseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/license");

                    if (!Directory.Exists(licenseFolder))
                        Directory.CreateDirectory(licenseFolder);

                    var licenseFileName = Guid.NewGuid() + Path.GetExtension(model.LicenseFile.FileName);
                    var licenseFullPath = Path.Combine(licenseFolder, licenseFileName);

                    using (var stream = new FileStream(licenseFullPath, FileMode.Create))
                    {
                        await model.LicenseFile.CopyToAsync(stream);
                    }

                    licensePath = "/uploads/license/" + licenseFileName;
                }

                // 📁 Save ID Proof File
                if (model.IdProofFile != null)
                {
                    var idFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/idproof");

                    if (!Directory.Exists(idFolder))
                        Directory.CreateDirectory(idFolder);

                    var idFileName = Guid.NewGuid() + Path.GetExtension(model.IdProofFile.FileName);
                    var idFullPath = Path.Combine(idFolder, idFileName);

                    using (var stream = new FileStream(idFullPath, FileMode.Create))
                    {
                        await model.IdProofFile.CopyToAsync(stream);
                    }

                    idProofPath = "/uploads/idproof/" + idFileName;
                }

                // SAVE TO DB
                var broker = new BrokerDetails
                {
                    UserId = user.UserId,
                    CompanyName = model.CompanyName,
                    BrokerLicenseNumber = model.BrokerLicenseNumber,
                    LicenseUploadPath = licensePath,
                    IdProofPath = idProofPath,
                    CreatedDate = DateTime.Now,
                    IsVerified = false
                };

                _context.BrokerDetails.Add(broker);
            }

            // 10. Builder logic
            if (role.RoleName.ToLower() == "builder")
            {
                if (string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    ModelState.AddModelError("CompanyName", "Company Name is required for Builder");
                    return View(model);
                }

                string? companyDocPath = null;

                // 📁 Save Company Document
                if (model.CompanyDocumentsFile != null)
                {
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/company");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var companyFileName = Guid.NewGuid() + Path.GetExtension(model.CompanyDocumentsFile.FileName);
                    var fullPath = Path.Combine(folder, companyFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.CompanyDocumentsFile.CopyToAsync(stream);
                    }

                    companyDocPath = "/uploads/company/" + companyFileName;
                }

                // SAVE TO DB
                var builder = new BuilderDetails
                {
                    UserId = user.UserId,
                    CompanyName = model.CompanyName,
                    CompanyDocumentsPath = companyDocPath, // 🔥 THIS IS THE MAIN FIX
                    CreatedDate = DateTime.Now,
                    IsVerified = false
                };

                _context.BuilderDetails.Add(builder);
            }

            // 11. Save everything
            await _context.SaveChangesAsync();

            // 12. OTP + Email
            await _otpService.GenerateAndSendOTPAsync(user.UserId, user.Email);
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

            // 13. Log
            LogActivity(user.UserId, "User Registered", "User", user.UserId, $"{user.FullName} registered");

            TempData["SuccessMessage"] = "Registration successful! Please verify your email.";

            return RedirectToAction("VerifyOTP", new { email = user.Email });
        }

        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string email, string otp)
        {
            if (string.IsNullOrEmpty(otp))
            {
                ModelState.AddModelError("", "Please enter OTP");
                ViewBag.Email = email;
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                ViewBag.Email = email;
                return View();
            }

            // Verify OTP
            var isVerified = _otpService.VerifyOTPAsync(user.UserId, otp).Result;

            if (isVerified)
            {
                user.IsVerified = true;
                user.UpdatedDate = DateTime.Now;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Email verified successfully! You can now login.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError("", "Invalid or expired OTP");
                ViewBag.Email = email;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResendOTP(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return RedirectToAction("Login");

            await _otpService.GenerateAndSendOTPAsync(user.UserId, email);

            TempData["SuccessMessage"] = "OTP resent successfully!";
            return RedirectToAction("VerifyOTP", new { email });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required");
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account has been deactivated");
                return View();
            }

            if (!user.IsVerified)
            {
                TempData["WarningMessage"] = "Please verify your email first";
                return RedirectToAction("VerifyOTP", new { email });
            }

            // Store user in session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            var roles = _context.UserRoles
                .Where(x => x.UserId == user.UserId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.RoleId,
                    (ur, r) => r.RoleName)
                .ToList();

            if (roles.Contains("Broker"))
            {
                var broker = _context.BrokerDetails.FirstOrDefault(b => b.UserId == user.UserId);

                if (broker == null || !broker.IsVerified)
                {
                    ModelState.AddModelError("", "Your documents are under verification. Please wait for admin approval.");
                    return View();
                }
            }

            if (roles.Contains("Builder"))
            {
                var builder = _context.BuilderDetails.FirstOrDefault(b => b.UserId == user.UserId);

                if (builder == null || !builder.IsVerified)
                {
                    ModelState.AddModelError("", "Your documents are under verification. Please wait for admin approval.");
                    return View();
                }
            }
            HttpContext.Session.SetString("UserRoles", string.Join(",", roles));
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);

            // Log activity
            LogActivity(user.UserId, "User Login", "User", user.UserId, "User logged in");

            // Redirect based on user type
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (roles.Contains("Builder"))
            {
                return RedirectToAction("Index", "Builder");
            }
            else if (roles.Contains("Broker"))
            {
                return RedirectToAction("Index", "Broker");
            }
            else if (roles.Contains("PropertyOwner"))
            {
                return RedirectToAction("Index", "PropertyOwner");
            }
            else
            {
                return RedirectToAction("Index", "Customer");
            }
        }

        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                LogActivity(userId.Value, "User Logout", "User", userId.Value, "User logged out");
            }

            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully";
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
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