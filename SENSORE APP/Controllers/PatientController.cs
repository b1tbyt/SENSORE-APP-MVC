using Microsoft.AspNetCore.Mvc;
using SENSORE_APP.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SENSORE_APP.Controllers
{
    public class PatientController : Controller
    {
        public const int ContactThreshold = 50;
        public const int HighPressureThreshold = 200;

        public IActionResult Index(string timeframe = "1h")
        {
            var vm = new PatientDashboardViewModel { SelectedTimeframe = timeframe };

            // Generate a simulated 32×32 heat map
            var rand = new Random();
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    vm.Heatmap[i, j] = (byte)rand.Next(1, 256);
                }
            }

            // Compute latest metrics
            byte maxValue = 0;
            int contactCells = 0;
            foreach (var val in vm.Heatmap)
            {
                if (val > maxValue) maxValue = val;
                if (val >= ContactThreshold) contactCells++;
            }
            vm.LatestPeakPressure = maxValue;
            vm.LatestContactArea = Math.Round(contactCells * 100.0 / (32 * 32), 1);
            vm.HighPressureAlert = vm.Heatmap.Cast<byte>().Any(v => v >= HighPressureThreshold);

            // Generate historical data
            int points = timeframe switch
            {
                "6h" => 36,
                "24h" => 48,
                "7d" => 28,
                _ => 12
            };
            DateTime now = DateTime.UtcNow;
            for (int i = points - 1; i >= 0; i--)
            {
                vm.Timestamps.Add(now.AddMinutes(-(points - 1 - i) * 10));
                vm.PeakPressureIndices.Add(10 + rand.NextDouble() * 40);
                vm.ContactAreaPercentages.Add(20 + rand.NextDouble() * 60);
            }

            // Compute risk score and category
            double avg = vm.PeakPressureIndices.Average();
            vm.RiskScore = Math.Round((avg - 10) / 40 * 10, 1);
            vm.RiskCategory = vm.RiskScore switch
            {
                < 3 => "Low",
                < 7 => "Moderate",
                _ => "High"
            };

            // Compute trends
            if (vm.PeakPressureIndices.Count >= 2)
            {
                int n = vm.PeakPressureIndices.Count;
                vm.PeakTrend = vm.PeakPressureIndices[n - 1] - vm.PeakPressureIndices[n - 2];
            }
            if (vm.ContactAreaPercentages.Count >= 2)
            {
                int n = vm.ContactAreaPercentages.Count;
                vm.ContactTrend = vm.ContactAreaPercentages[n - 1] - vm.ContactAreaPercentages[n - 2];
            }

            // NEW: schedule next reposition (randomly for demo)
            int minutesUntilNext = rand.Next(15, 46);
            vm.NextRepositionTime = now.AddMinutes(minutesUntilNext);
            vm.MinutesUntilReposition = minutesUntilNext;

            // Demo message thread
            vm.Messages = new List<Message>
            {
                new Message { Sender = "Clinician", Text = "Remember to shift weight every 30 minutes.", Timestamp = now.AddHours(-2) },
                new Message { Sender = "Patient",   Text = "I have felt pressure on my left hip.", Timestamp = now.AddHours(-1) }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNote(PatientDashboardViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model?.NewNote))
            {
                DateTime ts = model.NoteTimestamp ?? DateTime.UtcNow;
                TempData["NoteConfirmation"] = $"Note submitted for {ts:G}.";
            }
            return RedirectToAction(nameof(Index), new { timeframe = model?.SelectedTimeframe });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMessage(PatientDashboardViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model?.NewMessage))
            {
                TempData["MessageConfirmation"] = "Message sent to clinician.";
            }
            return RedirectToAction(nameof(Index), new { timeframe = model?.SelectedTimeframe });
        }

        public IActionResult GenerateReport()
        {
            return Content("Report generation is not yet implemented.");
        }
    }
}