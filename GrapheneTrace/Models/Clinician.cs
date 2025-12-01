using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models{

    public class Clinician : User
    {
        [StringLength(50)]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string Specialization { get; set; } = "General";

        [StringLength(100)]
        public string Department { get; set; } = "General Medicine";

        [StringLength(20)]
        [Phone]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        // Navigation property for patient-clinician relationships
        public virtual ICollection<PatientClinician> PatientClinicians { get; set; } = new List<PatientClinician>();
    }
}
