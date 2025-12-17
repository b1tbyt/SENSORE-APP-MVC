using GrapheneTrace.Models;
using System.Security.Cryptography;
using System.Text;

namespace GrapheneTrace.Data
{
    /// <summary>
    /// ApplicationDbSeeder - Database initialization with sample data.
    /// 
    /// PURPOSE:
    /// - Seeds database with initial test data for development
    /// - Creates sample users (Admin, Clinicians, Patients)
    /// - Establishes Patient-Clinician assignments
    /// - Generates initial audit log entries
    /// 
    /// USAGE:
    /// Call ApplicationDbSeeder.Seed(context) in Program.cs after EnsureCreated().
    /// 
    /// Author: 2402966
    /// </summary>
    public static class ApplicationDbSeeder
    {
        /// <summary>
        /// Seeds the database with initial data if empty.
        /// </summary>
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Users.Any())
            {
                Console.WriteLine("[Seeder] Database already seeded. Skipping.");
                return;
            }

            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  SEEDING DATABASE WITH SAMPLE DATA");
            Console.WriteLine("═══════════════════════════════════════════════════");

            // Create Admin
            var admin = new Admin
            {
                Email = "admin@sensore.com",
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = HashPassword("Admin123!"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-30),
                AccessLevel = "Super"
            };
            context.Admins.Add(admin);
            Console.WriteLine($"  ✓ Admin: {admin.Email}");

            // Create Clinicians
            var clinician1 = new Clinician
            {
                Email = "s.wilson@clinic.com",
                FirstName = "Sarah",
                LastName = "Wilson",
                PasswordHash = HashPassword("Clinician123!"),
                Role = "Clinician",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-21),
                LicenseNumber = "MED-2024-001",
                Specialization = "Wound Care",
                Department = "Dermatology",
                ContactPhone = "555-0101"
            };
            var clinician2 = new Clinician
            {
                Email = "m.chen@clinic.com",
                FirstName = "Michael",
                LastName = "Chen",
                PasswordHash = HashPassword("Clinician123!"),
                Role = "Clinician",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-14),
                LicenseNumber = "MED-2024-002",
                Specialization = "Geriatric Care",
                Department = "Internal Medicine",
                ContactPhone = "555-0102"
            };
            context.Clinicians.AddRange(clinician1, clinician2);
            Console.WriteLine($"  ✓ Clinicians: 2 created");

            // Create Patients
            var patient1 = new Patient
            {
                Email = "john.davis@patient.com",
                FirstName = "John",
                LastName = "Davis",
                PasswordHash = HashPassword("Patient123!"),
                Role = "Patient",
                IsActive =true,
                CreatedAt = DateTime.Now.AddDays(-10),
                MedicalCondition = "Type 2 Diabetes, Limited Mobility",
                MobilityLevel = "Limited"
            };
            var patient2 = new Patient
            {
                Email = "emma.r@patient.com",
                FirstName = "Emma",
                LastName = "Rodriguez",
                PasswordHash = HashPassword("Patient123!"),
                Role = "Patient",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-7),
                MedicalCondition = "Post-surgical recovery",
                MobilityLevel = "Bed-bound"
            };
            var patient3 = new Patient
            {
                Email = "robert.smith@patient.com",
                FirstName = "Robert",
                LastName = "Smith",
                PasswordHash = HashPassword("Patient123!"),
                Role = "Patient",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-3),
                MedicalCondition = "Spinal injury",
                MobilityLevel = "Wheelchair"
            };
            context.Patients.AddRange(patient1, patient2, patient3);
            Console.WriteLine($"  ✓ Patients: 3 created");

            // Save to get IDs
            context.SaveChanges();

            // Create Assignments
            context.PatientClinicians.AddRange(
                new PatientClinician { PatientId = patient1.UserId, ClinicianId = clinician1.UserId, AssignmentDate = DateTime.Now.AddDays(-10), Notes = "Primary wound care" },
                new PatientClinician { PatientId = patient2.UserId, ClinicianId = clinician1.UserId, AssignmentDate = DateTime.Now.AddDays(-7), Notes = "Post-surgical care" },
                new PatientClinician { PatientId = patient2.UserId, ClinicianId = clinician2.UserId, AssignmentDate = DateTime.Now.AddDays(-7), Notes = "Geriatric consultation" },
                new PatientClinician { PatientId = patient3.UserId, ClinicianId = clinician2.UserId, AssignmentDate = DateTime.Now.AddDays(-3), Notes = "Pressure ulcer prevention" }
            );
            Console.WriteLine($"  ✓ Assignments: 4 created");

            // Create Audit Logs
            context.AuditLogs.AddRange(
                new AuditLog { Action = "SYSTEM_INIT", Details = "System initialized", PerformedBy = "System", Timestamp = DateTime.Now.AddDays(-30), IpAddress = "127.0.0.1" },
                new AuditLog { Action = "CREATE_USER", Details = $"Created Admin: {admin.Email}", PerformedBy = "System", Timestamp = DateTime.Now.AddDays(-30), IpAddress = "127.0.0.1" },
                new AuditLog { Action = "CREATE_USER", Details = $"Created Clinician: Dr. Sarah Wilson", PerformedBy = admin.Email, Timestamp = DateTime.Now.AddDays(-21), IpAddress = "192.168.1.100" },
                new AuditLog { Action = "CREATE_USER", Details = $"Created Clinician: Dr. Michael Chen", PerformedBy = admin.Email, Timestamp = DateTime.Now.AddDays(-14), IpAddress = "192.168.1.100" },
                new AuditLog { Action = "CREATE_USER", Details = $"Created Patient: John Davis", PerformedBy = admin.Email, Timestamp = DateTime.Now.AddDays(-10), IpAddress = "192.168.1.100" },
                new AuditLog { Action = "CREATE_USER", Details = $"Created Patient: Emma Rodriguez", PerformedBy = admin.Email, Timestamp = DateTime.Now.AddDays(-7), IpAddress = "192.168.1.100" },
                new AuditLog { Action = "CREATE_USER", Details = $"Created Patient: Robert Smith", PerformedBy = admin.Email, Timestamp = DateTime.Now.AddDays(-3), IpAddress = "192.168.1.100" },
                new AuditLog { Action = "USER_LOGIN", Details = $"Admin logged in", PerformedBy = admin.Email, Timestamp = DateTime.Now.AddHours(-1), IpAddress = "::1" }
            );
            Console.WriteLine($"  ✓ Audit Logs: 8 created");

            context.SaveChanges();

            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  SEEDING COMPLETE!");
            Console.WriteLine("═══════════════════════════════════════════════════");
            Console.WriteLine("  LOGIN: admin@sensore.com / Admin123!");
            Console.WriteLine("═══════════════════════════════════════════════════\n");
        }

        private static string HashPassword(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
