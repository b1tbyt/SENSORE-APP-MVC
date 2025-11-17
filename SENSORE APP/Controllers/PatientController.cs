using Microsoft.AspNetCore.Mvc;
using SENSORE_APP.Models;
using System;
using System.Linq;

namespace SENSORE_APP.Controllers
{
    public class PatientController : Controller
    {
        private static readonly Random _random = new Random();

        // GET: /Patient/Index?timeframe=1h
        public IActionResult Index(string timeframe = "1h")
        {
            var vm = new PatientDashboardViewModel { SelectedTimeframe = timeframe };

            // Generate heatmap
            GenerateHeatmap(vm);

            // Generate historical data
            int dataPoints = GetDataPointsForTimeframe(timeframe);
            GenerateHistoricalData(vm, dataPoints);

            // Calculate risk score
            vm.RiskScore = CalculateRiskScore(vm.PeakPressureIndices);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNote(PatientDashboardViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model?.NewNote))
            {
                ViewBag.NoteConfirmation = "Note submitted successfully.";
                // TODO: save the note to a data store here.
            }
            return RedirectToAction(nameof(Index), new { timeframe = model?.SelectedTimeframe });
        }

        private void GenerateHeatmap(PatientDashboardViewModel vm)
        {
            for (int i = 0; i < vm.HeatmapSize; i++)
            {
                for (int j = 0; j < vm.HeatmapSize; j++)
                {
                    vm.Heatmap[i, j] = (byte)_random.Next(1, 200);
                }
            }
        }

        private int GetDataPointsForTimeframe(string timeframe) => timeframe switch
        {
            "6h" => 36,
            "24h" => 48,
            "7d" => 28,
            _ => 12
        };

        private void GenerateHistoricalData(PatientDashboardViewModel vm, int dataPoints)
        {
            DateTime now = DateTime.UtcNow;
            for (int i = dataPoints - 1; i >= 0; i--)
            {
                vm.Timestamps.Add(now.AddMinutes(-(dataPoints - 1 - i) * 10));
                vm.PeakPressureIndices.Add(10 + _random.NextDouble() * 40);
                vm.ContactAreaPercentages.Add(20 + _random.NextDouble() * 60);
            }
        }

        private double CalculateRiskScore(System.Collections.Generic.List<double> pressureIndices)
        {
            if (pressureIndices.Count == 0) return 0;
            double avg = pressureIndices.Average();
            return Math.Round((avg - 10) / 40 * 10, 1);
        }
    }
}                       