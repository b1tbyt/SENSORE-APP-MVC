using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Models;

namespace GrapheneTrace.Data
{
    /// <summary>
    /// ApplicationDbContext - Entity Framework Core database context
    /// Manages database connections and entity configurations
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // User entities
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Clinician> Clinicians { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // Assignment entity
        public DbSet<PatientClinician> PatientClinicians { get; set; }

        // Audit logging
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPH inheritance
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Patient>("Patient")
                .HasValue<Clinician>("Clinician")
                .HasValue<Admin>("Admin");

            // Unique email constraint
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // PatientClinician relationships
            modelBuilder.Entity<PatientClinician>()
                .HasOne(pc => pc.Patient)
                .WithMany(p => p.PatientClinicians)
                .HasForeignKey(pc => pc.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientClinician>()
                .HasOne(pc => pc.Clinician)
                .WithMany(c => c.PatientClinicians)
                .HasForeignKey(pc => pc.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique Patient-Clinician combination
            modelBuilder.Entity<PatientClinician>()
                .HasIndex(pc => new { pc.PatientId, pc.ClinicianId })
                .IsUnique();
        }
    }
}
