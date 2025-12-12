using Microsoft.VisualStudio.TestTools.UnitTesting;
using SENSORE_APP.Services.Strategies;

namespace SensoreApp.Tests
{
    [TestClass]
    public class RiskCalculatorTests
    {
        [TestMethod]
        public void RiskCalculator_ReturnsHighRisk_ForHighPressures()
        {
            // Arrange
            byte[,] grid = new byte[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    grid[i, j] = 200; // Simulated high pressure (0-255 range)
                }
            }

            var analyzer = new StrategyBasedRiskAnalyzer();
            var result = analyzer.Analyze(grid);

            // Act & Assert
            // With all cells at 200, peak pressure strategy returns (200/255)*10 ≈ 7.8
            Assert.IsTrue(result.RiskScore > 5, "Risk score should be high for uniformly high pressures (200+).");
            Assert.IsNotNull(result.StrategyUsed, "Strategy name should be populated.");
        }

        [TestMethod]
        public void RiskCalculator_ReturnsLowRisk_ForLowPressures()
        {
            // Arrange
            byte[,] grid = new byte[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    grid[i, j] = 20; // Simulated low pressure
                }
            }

            var analyzer = new StrategyBasedRiskAnalyzer();
            var result = analyzer.Analyze(grid);

            // Act & Assert
            // With all cells at 20, peak pressure strategy returns (20/255)*10 ≈ 0.78
            Assert.IsTrue(result.RiskScore < 3, "Risk score should be low for low pressures.");
            Assert.IsFalse(result.RequiresIntervention, "Low pressure should not require intervention.");
        }

        [TestMethod]
        public void RiskCalculator_DetectsHotspots()
        {
            // Arrange
            byte[,] grid = new byte[32, 32];
            
            // Create hotspots covering more than 25% of the grid to trigger intervention
            // 32*32 = 1024 cells, need > 256 cells (>25%) at hotspot level
            // 17x17 = 289 cells = 28.2% (satisfies > 25%)
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    grid[i, j] = 240; // Critical pressure (hotspot threshold: 180+)
                }
            }

            var analyzer = new StrategyBasedRiskAnalyzer();
            analyzer.SetStrategy(new HotspotConcentrationStrategy());
            var result = analyzer.Analyze(grid);

            // Act & Assert
            Assert.IsTrue(result.RiskScore > 0, "Hotspot should be detected.");
            Assert.IsTrue(result.RequiresIntervention, "Hotspots covering >25% of grid should trigger intervention.");
        }

        [TestMethod]
        public void RiskCalculator_MultiStrategyAnalysis()
        {
            // Arrange
            byte[,] grid = new byte[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    grid[i, j] = 100; // Moderate pressure
                }
            }

            var analyzer = new StrategyBasedRiskAnalyzer();
            var result = analyzer.AnalyzeWithAllStrategies(grid);

            // Act & Assert
            Assert.IsTrue(result.RiskScore >= 0, "Risk score should be non-negative.");
            Assert.IsTrue(result.RiskScore <= 10, "Risk score should be within 0-10 range.");
            Assert.AreEqual("Multi-Strategy Average", result.StrategyUsed, "Strategy should be Multi-Strategy Average.");
        }
    }
}