using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
  
    public class PatientClinician
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ClinicianId { get; set; }

        [Display(Name = "Assignment Date")]
        public DateTime AssignmentDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("ClinicianId")]
        public virtual Clinician? Clinician { get; set; }
    }
}
