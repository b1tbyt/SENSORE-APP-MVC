using System.ComponentModel.DataAnnotations;

namespace SensoreApp.Models

{
    public class PatientRecord
    {
        public int Id { get; set; }

        [Required]

        public int PatientID { get; set; }
        public string? PatientName { get; set; }

        public string? MedicalStatus { get; set; }

        public string? MedicalReport { get; set; }
    }
}
