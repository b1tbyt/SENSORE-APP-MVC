using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using System.Security.Cryptography;
using System.Text;
using GrapheneTrace.Hubs;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// SIGNALR & SENSOR SERVICES 
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

// Enable Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });


builder.Services.AddSingleton<PressureDataService>();

builder.Services.AddSingleton<HeatmapBroadcastService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("GrapheneTraceDb"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
    context.Database.EnsureCreated();
       
    if (!context.Users.Any())
    {
        SeedDatabase(context);
    }

}


void SeedDatabase(ApplicationDbContext context)
{
        string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    // Create System Administrator
    var admin = new Admin
    {
        Email = "admin@sensore.com",
        FirstName = "System",
        LastName = "Administrator",
        PasswordHash = HashPassword("Admin123!"),
        Role = "Admin",
        IsActive = true,
        CreatedAt = DateTime.Now,
        AccessLevel = "Full"
    };
    context.Admins.Add(admin);

    // Create Clinicians
    var clinician1 = new Clinician
    {
        Email = "s.wilson@clinic.com",
        FirstName = "Sarah",
        LastName = "Wilson",
        PasswordHash = HashPassword("Clinician123!"),
        Role = "Clinician",
        IsActive = true,
        CreatedAt = DateTime.Now,
        LicenseNumber = "MED-2024-001",
        Specialization = "Wound Care",
        Department = "Dermatology",
        ContactPhone = "555-0101"
    };
    context.Clinicians.Add(clinician1);

    var clinician2 = new Clinician
    {
        Email = "m.chen@clinic.com",
        FirstName = "Michael",
        LastName = "Chen",
        PasswordHash = HashPassword("Clinician123!"),
        Role = "Clinician",
        IsActive = true,
        CreatedAt = DateTime.Now,
        LicenseNumber = "MED-2024-002",
        Specialization = "Geriatric Care",
        Department = "Internal Medicine",
        ContactPhone = "555-0102"
    };
    context.Clinicians.Add(clinician2);

    // Create Patients
    var patient1 = new Patient
    {
        Email = "john.davis@patient.com",
        FirstName = "John",
        LastName = "Davis",
        PasswordHash = HashPassword("Patient123!"),
        Role = "Patient",
        IsActive = true,
        CreatedAt = DateTime.Now,
        MedicalCondition = "Type 2 Diabetes, Limited Mobility",
        MobilityLevel = "Limited"
    };
    context.Patients.Add(patient1);

    var patient2 = new Patient
    {
        Email = "emma.r@patient.com",
        FirstName = "Emma",
        LastName = "Rodriguez",
        PasswordHash = HashPassword("Patient123!"),
        Role = "Patient",
        IsActive = true,
        CreatedAt = DateTime.Now,
        MedicalCondition = "Post-surgical recovery",
        MobilityLevel = "Bed-bound"
    };
    context.Patients.Add(patient2);

    var patient3 = new Patient
    {
        Email = "robert.smith@patient.com",
        FirstName = "Robert",
        LastName = "Smith",
        PasswordHash = HashPassword("Patient123!"),
        Role = "Patient",
        IsActive = true,
        CreatedAt = DateTime.Now,
        MedicalCondition = "Spinal injury, wheelchair user",
        MobilityLevel = "Wheelchair"
    };
    context.Patients.Add(patient3);

    context.SaveChanges();

    // Create Patient-Clinician Assignments
    var assignments = new List<PatientClinician>
    {
        new PatientClinician
        {
            PatientId = patient1.UserId,
            ClinicianId = clinician1.UserId,
            AssignmentDate = DateTime.Now.AddDays(-7),
            Notes = "Primary care - wound monitoring"
        },
        new PatientClinician
        {
            PatientId = patient2.UserId,
            ClinicianId = clinician1.UserId,
            AssignmentDate = DateTime.Now.AddDays(-3),
            Notes = "Post-surgical wound care"
        },
        new PatientClinician
        {
            PatientId = patient2.UserId,
            ClinicianId = clinician2.UserId,
            AssignmentDate = DateTime.Now.AddDays(-3),
            Notes = "Geriatric consultation"
        },
        new PatientClinician
        {
            PatientId = patient3.UserId,
            ClinicianId = clinician2.UserId,
            AssignmentDate = DateTime.Now.AddDays(-1),
            Notes = "Pressure ulcer prevention program"
        }
    };

    context.PatientClinicians.AddRange(assignments);
    context.SaveChanges();

    Console.WriteLine("Database seeded successfully with:");
    Console.WriteLine($"  - {context.Admins.Count()} Administrator(s)");
    Console.WriteLine($"  - {context.Clinicians.Count()} Clinician(s)");
    Console.WriteLine($"  - {context.Patients.Count()} Patient(s)");
    Console.WriteLine($"  - {context.PatientClinicians.Count()} Assignment(s)");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<PressureHub>("/sensorHub");


app.Run();
