using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{
    
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        
        [Required]
        [StringLength(1000)]
        public string Details { get; set; } = string.Empty;

        
        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;

        
        public DateTime Timestamp { get; set; } = DateTime.Now;

        
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;
    }

    
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
