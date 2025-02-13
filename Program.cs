using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Assignment1.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Assignment1.Pages;
using Assignment1.Services;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<AuditLogger>();

builder.Services.AddSingleton<SessionTracker>();

builder.Services.AddSingleton<EmailService>();

// Register the Encryption service
builder.Services.AddScoped<Encryption>();

// ? Register AuthDbContext with connection string
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString")));

// ? Register Identity with AuthDbContext
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 3; // Lockout after 3 failed attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Lockout duration
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // Secure the session cookie from JavaScript
    options.Cookie.IsEssential = true; // Mark the cookie as essential
    options.IdleTimeout = TimeSpan.FromSeconds(10); // Set session expiration
});



builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;         // At least one number
    options.Password.RequireLowercase = true;     // At least one lowercase letter
    options.Password.RequireUppercase = true;     // At least one uppercase letter
    options.Password.RequireNonAlphanumeric = true; // At least one special character
    options.Password.RequiredLength = 12;         // Minimum length of 12 characters
});

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
