using System;
using System.Collections.Generic;

namespace SENSORE_APP.Models
{
    /// View model used by the patient dashboard.
    public class PatientDashboardViewModel
    {
        private const int DEFAULT_HEATMAP_SIZE = 32;
        private const int DEFAULT_CELL_PIXEL_SIZE = 16;

        public byte[,] Heatmap { get; set; } = new byte[DEFAULT_HEATMAP_SIZE, DEFAULT_HEATMAP_SIZE];
        public List<DateTime> Timestamps { get; set; } = new List<DateTime>();
        public List<double> PeakPressureIndices { get; set; } = new List<double>();
        public List<double> ContactAreaPercentages { get; set; } = new List<double>();
        public double RiskScore { get; set; }
        public string SelectedTimeframe { get; set; } = "1h";
        public string NewNote { get; set; }

        public int HeatmapSize => DEFAULT_HEATMAP_SIZE;
        public int CellPixelSize => DEFAULT_CELL_PIXEL_SIZE;
        public string[] TimeframeOptions => new[] { "1h", "6h", "24h", "7d" };

        /// <summary>
        /// Converts a heatmap value (1-255) to an RGB color string.
        /// Red indicates high pressure, green indicates low pressure.
        /// </summary>
        public string GetHeatmapColor(int row, int col)
        {
            byte val = Heatmap[row, col];
            double ratio = (val - 1) / 254.0;
            int red = (int)(255 * ratio);
            int green = (int)(255 * (1 - ratio));
            return $"rgb({red}, {green}, 0)";
        }

        /// <summary>
        /// Returns a color based on the current risk score.
        /// Green (low risk) → Yellow (medium) → Red (high risk).
        /// </summary>
        public string GetRiskColor()
        {
            return RiskScore < 4 ? "#00aa00" : RiskScore < 7 ? "#f3c800" : "#d9534f";
        }
    }
}                   