using System.Collections.Generic;

namespace SensoreApp.Models
{
    public class ClinicianDashboardVM
    {
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int TotalPatients { get; set; }
        public int PendingAppointments { get; set; }

      
        public List<int>? AppointmentsLast7Days { get; set; }
        public List<int>? AppointmentStatusCounts { get; set; }
        public List<Appointment>? TodaysAppointments { get; set; }
        public int LatestHeartRate { get; set; }
        public float LatestTemperature { get; set; }
        public int LatestOxygen { get; set; }

    }
}
