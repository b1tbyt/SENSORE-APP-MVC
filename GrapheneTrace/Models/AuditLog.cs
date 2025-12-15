using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{
    /// <summary>
    /// AuditLog - Tracks all administrative actions for security and compliance.
    /// Provides complete audit trail of who did what and when.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        /// <summary>
        /// Type of action performed (e.g., CREATE_USER, DELETE_USER, UPDATE_USER)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the action
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Username/email of the admin who performed the action
        /// </summary>
        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// When the action was performed
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// IP address of the client (for security tracking)
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// ReportViewModel - Used for the Reports page to display analytics
    /// </summary>
    public class ReportViewModel
    {
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int TotalClinicians { get; set; }
        public int ActiveClinicians { get; set; }
        public int TotalAssignments { get; set; }

        public Dictionary<string, int> MonthlyRegistrations { get; set; } = new();
        public Dictionary<string, int> RoleDistribution { get; set; } = new();
        public List<AuditLog> RecentActivity { get; set; } = new();
    }
}
