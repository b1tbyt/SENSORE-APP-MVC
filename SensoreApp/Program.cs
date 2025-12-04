using Microsoft.EntityFrameworkCore;
using SensoreApp.Data;
using SensoreApp.Patterns;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------
// DATABASE
// ---------------------------
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------
// MVC + RAZOR PAGES
// ---------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<VitalSignMonitor>(_ => VitalSignMonitor.Instance);
builder.Services.AddRazorPages();

var app = builder.Build();

// ---------------------------
// PIPELINE
// ---------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// NOTE: Identity and authentication removed so no UseAuthentication()/UseAuthorization()

// ---------------------------
// ROUTING
// ---------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Clinician}/{action=Dashboard}/{id?}");

app.MapRazorPages();

app.Run();