using GoPost.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity (replace IdentityUser with your custom user class if you have one, in this case ApplicationUser)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => // IMPORTANT: Register Identity
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
    // Password settings.  It's good to have these, even with your previous settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.  Good to have these as well.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>() // IMPORTANT:  Use EntityFrameworkStores
    .AddDefaultTokenProviders(); // Add this if you need password resets, etc.

// Configure Identity cookie settings for security.  No Change here, but adding comments.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Prevent JavaScript access to cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are only sent over HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Restrict cookies to same-site requests
});

// Add services to the container.  No Change here.
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddScoped<GoPost.Controllers.NotificationsController>();


var app = builder.Build();

// Configure the HTTP request pipeline.  No Changes here, but adding comments.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Enable migrations endpoint in development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use error handling for production
    app.UseHsts(); // Enable HSTS (HTTP Strict Transport Security)
}

// Redirect HTTP to HTTPS.  No Changes here.
app.UseHttpsRedirection();

// Serve static files (e.g., CSS, JS, images). No Changes.
app.UseStaticFiles();

// Enable routing. No Changes.
app.UseRouting();

// Enable authentication and authorization. No Changes.
app.UseAuthentication();
app.UseAuthorization();

// Map default controller route. No Changes.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages. No Changes.
app.MapRazorPages();

// Run the application. No Changes.
app.Run();
