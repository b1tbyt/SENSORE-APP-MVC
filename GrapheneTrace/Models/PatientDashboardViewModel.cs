namespace GrapheneTrace.Models
{
    public class PatientDashboardViewModel
    {
        public byte[,] Heatmap { get; set; } = new byte[32, 32];
        public List<DateTime> Timestamps { get; set; } = new();
        public List<double> PeakPressureIndices { get; set; } = new();
        public List<double> ContactAreaPercentages { get; set; } = new();

        public double LatestPeakPressure { get; set; }
        public double LatestContactArea { get; set; }
        public double RiskScore { get; set; }
        public string RiskCategory { get; set; } = string.Empty;
        public bool HighPressureAlert { get; set; }

        public double PeakTrend { get; set; }
        public double ContactTrend { get; set; }

        public List<Message> Messages { get; set; } = new();
        public string SelectedTimeframe { get; set; } = "1h";
        public string NewNote { get; set; } = string.Empty;
        public DateTime? NoteTimestamp { get; set; }
        public string NewMessage { get; set; } = string.Empty;

        // NEW: next reposition time and minutes remaining
        public DateTime NextRepositionTime { get; set; }
        public double MinutesUntilReposition { get; set; }
    }
}