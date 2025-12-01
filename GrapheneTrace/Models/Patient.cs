using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{
    
    public class Patient : User
    {
        [StringLength(500)]
        [Display(Name = "Medical Condition")]
        public string MedicalCondition { get; set; } = "Not specified";

        [StringLength(50)]
        [Display(Name = "Mobility Level")]
        public string MobilityLevel { get; set; } = "Normal";

        // Navigation property for patient-clinician relationships
        public virtual ICollection<PatientClinician> PatientClinicians { get; set; } = new List<PatientClinician>();
    }
}
