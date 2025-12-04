
using System.ComponentModel.DataAnnotations;
namespace SensoreApp.Models

{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientID { get; set; }
        [Required]
        public string? PatientName { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";
    }
}