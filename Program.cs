using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealEstateManagement.Models;
using RealEstateManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Database Configuration
builder.Services.AddDbContext<RealestatemanagementContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 23)))); // merged MySQL version from your first code

// Email Configuration
// Email Configuration (safe version)
var smtpPortString = builder.Configuration["EmailConfiguration:SmtpPort"];
int smtpPort = 587; // default SMTP port
if (!string.IsNullOrEmpty(smtpPortString) && int.TryParse(smtpPortString, out int parsedPort))
{
    smtpPort = parsedPort;
}

var emailConfig = new EmailConfiguration
{
    SmtpServer = builder.Configuration["EmailConfiguration:SmtpServer"] ?? "smtp.gmail.com",
    SmtpPort = smtpPort,
    SenderName = builder.Configuration["EmailConfiguration:SenderName"] ?? "Real Estate Management",
    SenderEmail = builder.Configuration["EmailConfiguration:SenderEmail"] ?? "23bmiit009@example.com",
    SenderPassword = builder.Configuration["EmailConfiguration:SenderPassword"] ?? "setnzpelkkraqpvw",
    EnableSSL = bool.TryParse(builder.Configuration["EmailConfiguration:EnableSSL"], out bool ssl) ? ssl : true
};

builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>(); // fixed interface name casing
builder.Services.AddScoped<INotificationService, NotificationService>();

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "RealEstateSession";
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RealestatemanagementContext>();

    if (!context.Users.Any(u => u.Email == "aakar13@gmail.com"))
    {
        var admin = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "aakar13@gmail.com",
            PhoneNumber = "9824008282",
            Address = "Gokulnagar, bhatar",
            City = "Surat",
            State = "Gujarat",
            Country = "India",
            PasswordHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes("admin123"))
            ),
            IsActive = true,
            IsVerified = true,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };

        context.Users.Add(admin);
        context.SaveChanges();

        // Assign Admin Role (Assuming RoleId = 1)
        context.UserRoles.Add(new UserRole
        {
            UserId = admin.UserId,
            RoleId = 5
        });

        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Static assets mapping (from first code)
app.MapStaticAssets();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RealestatemanagementContext>();
    context.Database.Migrate();
}

app.Run();