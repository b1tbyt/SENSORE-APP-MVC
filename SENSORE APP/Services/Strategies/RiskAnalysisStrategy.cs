using System;
using System.Collections.Generic;
using System.Linq;

namespace SENSORE_APP.Services.Strategies
{
    /// <summary>
    /// Strategy Pattern (Behavioral)
    /// Defines a family of algorithms, encapsulates each one, and makes them interchangeable.
    /// This allows risk analysis to use different algorithms without changing the client code.
    /// </summary>

    /// <summary>
    /// Strategy interface for heatmap analysis
    /// Different strategies calculate risk scores differently
    /// </summary>
    public interface IRiskAnalysisStrategy
    {
        /// <summary>
        /// Calculates risk score based on heatmap data
        /// </summary>
        double CalculateRiskScore(byte[,] heatmap);

        /// <summary>
        /// Gets a human-readable description of the strategy
        /// </summary>
        string GetStrategyName();

        /// <summary>
        /// Determines if patient needs immediate intervention
        /// </summary>
        bool RequiresImmediateIntervention(byte[,] heatmap);
    }

    /// <summary>
    /// Peak Pressure Strategy
    /// Focuses on the maximum pressure values
    /// </summary>
    public class PeakPressureStrategy : IRiskAnalysisStrategy
    {
        private const int HighPressureThreshold = 200;
        private const int CriticalPressureThreshold = 240;

        public double CalculateRiskScore(byte[,] heatmap)
        {
            if (heatmap == null || heatmap.Length == 0)
                return 0;

            byte maxPressure = 0;
            foreach (byte val in heatmap)
            {
                if (val > maxPressure) maxPressure = val;
            }

            // Risk score 0-10 based on max pressure
            // 0-100: Low (0-3)
            // 100-200: Moderate (3-7)
            // 200+: High (7-10)
            return (maxPressure / 255.0) * 10.0;
        }

        public string GetStrategyName()
        {
            return "Peak Pressure Analysis";
        }

        public bool RequiresImmediateIntervention(byte[,] heatmap)
        {
            byte maxPressure = 0;
            foreach (byte val in heatmap)
            {
                if (val > maxPressure) maxPressure = val;
            }
            return maxPressure >= CriticalPressureThreshold;
        }
    }

    /// <summary>
    /// Contact Area Strategy
    /// Focuses on the percentage of cells detecting pressure
    /// </summary>
    public class ContactAreaStrategy : IRiskAnalysisStrategy
    {
        private const int ContactThreshold = 50;

        public double CalculateRiskScore(byte[,] heatmap)
        {
            if (heatmap == null || heatmap.Length == 0)
                return 0;

            int contactCells = 0;
            foreach (byte val in heatmap)
            {
                if (val >= ContactThreshold) contactCells++;
            }

            double contactPercentage = (contactCells * 100.0) / (heatmap.GetLength(0) * heatmap.GetLength(1));

            // Risk score based on contact area percentage
            // 0-33%: Low (0-3)
            // 33-66%: Moderate (3-7)
            // 66-100%: High (7-10)
            return (contactPercentage / 100.0) * 10.0;
        }

        public string GetStrategyName()
        {
            return "Contact Area Analysis";
        }

        public bool RequiresImmediateIntervention(byte[,] heatmap)
        {
            int contactCells = 0;
            foreach (byte val in heatmap)
            {
                if (val >= ContactThreshold) contactCells++;
            }
            double contactPercentage = (contactCells * 100.0) / (heatmap.GetLength(0) * heatmap.GetLength(1));
            return contactPercentage > 80; // More than 80% contact
        }
    }

    /// <summary>
    /// Hotspot Concentration Strategy
    /// Focuses on high-pressure areas (hotspots)
    /// </summary>
    public class HotspotConcentrationStrategy : IRiskAnalysisStrategy
    {
        private const int HotspotThreshold = 180;

        public double CalculateRiskScore(byte[,] heatmap)
        {
            if (heatmap == null || heatmap.Length == 0)
                return 0;

            int hotspotCells = 0;
            double totalPressure = 0;

            foreach (byte val in heatmap)
            {
                if (val >= HotspotThreshold) hotspotCells++;
                totalPressure += val;
            }

            double hotspotPercentage = (hotspotCells * 100.0) / (heatmap.GetLength(0) * heatmap.GetLength(1));
            double averagePressure = totalPressure / (heatmap.GetLength(0) * heatmap.GetLength(1));

            // Combined score: hotspot concentration + average pressure
            double riskFromHotspots = (hotspotPercentage / 100.0) * 5.0;
            double riskFromAverage = (averagePressure / 255.0) * 5.0;

            return riskFromHotspots + riskFromAverage;
        }

        public string GetStrategyName()
        {
            return "Hotspot Concentration Analysis";
        }

        public bool RequiresImmediateIntervention(byte[,] heatmap)
        {
            int hotspotCells = 0;
            foreach (byte val in heatmap)
            {
                if (val >= HotspotThreshold) hotspotCells++;
            }
            return hotspotCells > (heatmap.GetLength(0) * heatmap.GetLength(1)) * 0.25; // More than 25% hotspots
        }
    }

    /// <summary>
    /// Pressure Distribution Strategy
    /// Focuses on variance and uniformity of pressure
    /// </summary>
    public class PressureDistributionStrategy : IRiskAnalysisStrategy
    {
        public double CalculateRiskScore(byte[,] heatmap)
        {
            if (heatmap == null || heatmap.Length == 0)
                return 0;

            double mean = CalculateMean(heatmap);
            double stdDev = CalculateStandardDeviation(heatmap, mean);

            // High variance = poor distribution = higher risk
            // Normalize std dev to 0-10 scale
            double normalizedStdDev = Math.Min(10, (stdDev / 50.0) * 10.0);

            return normalizedStdDev;
        }

        public string GetStrategyName()
        {
            return "Pressure Distribution Analysis";
        }

        public bool RequiresImmediateIntervention(byte[,] heatmap)
        {
            double mean = CalculateMean(heatmap);
            double stdDev = CalculateStandardDeviation(heatmap, mean);

            // Extreme variance indicates uneven pressure distribution
            return stdDev > 100;
        }

        private double CalculateMean(byte[,] heatmap)
        {
            double sum = 0;
            foreach (byte val in heatmap)
            {
                sum += val;
            }
            return sum / heatmap.Length;
        }

        private double CalculateStandardDeviation(byte[,] heatmap, double mean)
        {
            double sumSquaredDifferences = 0;
            foreach (byte val in heatmap)
            {
                sumSquaredDifferences += Math.Pow(val - mean, 2);
            }
            return Math.Sqrt(sumSquaredDifferences / heatmap.Length);
        }
    }

    /// <summary>
    /// Risk analyzer that uses strategy pattern
    /// Allows switching between different analysis algorithms
    /// </summary>
    public class StrategyBasedRiskAnalyzer
    {
        private IRiskAnalysisStrategy _strategy;
        private readonly List<IRiskAnalysisStrategy> _availableStrategies;

        public StrategyBasedRiskAnalyzer()
        {
            _availableStrategies = new List<IRiskAnalysisStrategy>
            {
                new PeakPressureStrategy(),
                new ContactAreaStrategy(),
                new HotspotConcentrationStrategy(),
                new PressureDistributionStrategy()
            };

            // Default strategy
            _strategy = _availableStrategies[0];
        }

        /// <summary>
        /// Sets the analysis strategy
        /// </summary>
        public void SetStrategy(IRiskAnalysisStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Sets strategy by name
        /// </summary>
        public bool SetStrategyByName(string strategyName)
        {
            var strategy = _availableStrategies.FirstOrDefault(s => s.GetStrategyName() == strategyName);
            if (strategy != null)
            {
                _strategy = strategy;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Analyzes heatmap using current strategy
        /// </summary>
        public RiskAnalysisResult Analyze(byte[,] heatmap)
        {
            return new RiskAnalysisResult
            {
                RiskScore = _strategy.CalculateRiskScore(heatmap),
                StrategyUsed = _strategy.GetStrategyName(),
                RequiresIntervention = _strategy.RequiresImmediateIntervention(heatmap),
                AnalysisTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Gets all available strategies
        /// </summary>
        public IEnumerable<string> GetAvailableStrategies()
        {
            return _availableStrategies.Select(s => s.GetStrategyName());
        }

        /// <summary>
        /// Performs multi-strategy analysis and returns average
        /// </summary>
        public RiskAnalysisResult AnalyzeWithAllStrategies(byte[,] heatmap)
        {
            var results = _availableStrategies
                .Select(s => s.CalculateRiskScore(heatmap))
                .ToList();

            bool anyRequiresIntervention = _availableStrategies
                .Any(s => s.RequiresImmediateIntervention(heatmap));

            return new RiskAnalysisResult
            {
                RiskScore = results.Average(),
                StrategyUsed = "Multi-Strategy Average",
                RequiresIntervention = anyRequiresIntervention,
                AnalysisTime = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Result of risk analysis
    /// </summary>
    public class RiskAnalysisResult
    {
        public double RiskScore { get; set; }
        public string StrategyUsed { get; set; }
        public bool RequiresIntervention { get; set; }
        public DateTime AnalysisTime { get; set; }
    }
}
