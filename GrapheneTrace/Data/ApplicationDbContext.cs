using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Models;

namespace GrapheneTrace.Data
{
    /// <summary>
    /// ApplicationDbContext - Entity Framework Core database context
    /// Manages database connections and entity configurations
    /// Uses Table-Per-Hierarchy (TPH) inheritance for User types
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // Constructor for dependency injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for each entity type
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Clinician> Clinicians { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PatientClinician> PatientClinicians { get; set; }

        /// <summary>
        /// Configure entity relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPH inheritance - all user types in single table with discriminator
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Patient>("Patient")
                .HasValue<Clinician>("Clinician")
                .HasValue<Admin>("Admin");

            // Configure unique constraint on Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure PatientClinician relationships
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

            // Configure unique constraint on Patient-Clinician combination
            modelBuilder.Entity<PatientClinician>()
                .HasIndex(pc => new { pc.PatientId, pc.ClinicianId })
                .IsUnique();
        }
    }
}
