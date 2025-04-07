using GoPost.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity with secure cookie settings
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true; // Ensure unique email addresses for users
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity cookie settings for security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Prevent JavaScript access to cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are only sent over HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Restrict cookies to same-site requests
});

// Add MVC and controllers
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Create a scope to resolve scoped services (optional debug check)
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
    if (userManager == null)
    {
        Console.WriteLine("UserManager<ApplicationUser> is NOT registered!");
    }
    else
    {
        Console.WriteLine("UserManager<ApplicationUser> is registered.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Enable migrations endpoint in development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use error handling for production
    app.UseHsts(); // Enable HSTS (HTTP Strict Transport Security)
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Serve static files (e.g., CSS, JS, images)
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages
app.MapRazorPages();

// Run the application
app.Run();