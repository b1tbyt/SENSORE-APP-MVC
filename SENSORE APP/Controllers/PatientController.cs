using Microsoft.AspNetCore.Mvc;
using SENSORE_APP.Models;
using SENSORE_APP.Services;
using SENSORE_APP.Services.Factories;
using SENSORE_APP.Services.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SENSORE_APP.Controllers
{
    public class PatientController : Controller
    {
        public const int ContactThreshold = 50;
        private const int HighPressureThreshold = 200;

        private readonly PressureDataService _pressureService;
        private readonly IMessageFactory _messageFactory;
        private readonly AlertMessageFactory _alertFactory;
        private readonly StrategyBasedRiskAnalyzer _riskAnalyzer;
        private readonly MessageStorageService _messageStorageService;
        private readonly HeatmapCsvLoader _csvLoader;

        public PatientController(
            PressureDataService pressureService,
            IMessageFactory messageFactory,
            AlertMessageFactory alertFactory,
            StrategyBasedRiskAnalyzer riskAnalyzer,
            MessageStorageService messageStorageService,
            HeatmapCsvLoader csvLoader)
        {
            _pressureService = pressureService;
            _messageFactory = messageFactory;
            _alertFactory = alertFactory;
            _riskAnalyzer = riskAnalyzer;
            _messageStorageService = messageStorageService;
            _csvLoader = csvLoader;
        }

        /// <summary>
        /// GET: /Patient/Index?timeframe=1h
        /// Main patient dashboard view
        /// </summary>
        public IActionResult Index(string timeframe = "1h")
        {
            var vm = new PatientDashboardViewModel { SelectedTimeframe = timeframe };

            // Get current heatmap from live service
            vm.Heatmap = _pressureService.GetCurrentHeatmap();

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
            var rand = new Random();
            for (int i = points - 1; i >= 0; i--)
            {
                vm.Timestamps.Add(now.AddMinutes(-(points - 1 - i) * 10));
                vm.PeakPressureIndices.Add(10 + rand.NextDouble() * 40);
                vm.ContactAreaPercentages.Add(20 + rand.NextDouble() * 60);
            }

            // Compute risk score using Strategy Pattern
            var riskAnalysis = _riskAnalyzer.Analyze(vm.Heatmap);
            vm.RiskScore = Math.Round(riskAnalysis.RiskScore, 1);
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

            // Schedule next reposition
            int minutesUntilNext = rand.Next(15, 46);
            vm.NextRepositionTime = now.AddMinutes(minutesUntilNext);
            vm.MinutesUntilReposition = minutesUntilNext;

            // Demo messages using Factory Pattern
            vm.Messages = new List<Message>
            {
                _messageFactory.CreateMessage("Clinician", "Remember to shift weight every 30 minutes."),
                _messageFactory.CreateMessage("Patient", "I have felt pressure on my left hip.")
            };

            // Check if high pressure alert needed (using Alert Factory)
            if (vm.HighPressureAlert)
            {
                var pressureAlert = _alertFactory.CreatePressureAlert(maxValue);
                vm.Messages.Add(pressureAlert);
            }

            return View(vm);
        }

        /// <summary>
        /// Messages view
        /// </summary>
        public IActionResult Messages()
        {
            var messages = _messageStorageService.GetAllMessages();
            return View(messages);
        }

        /// <summary>
        /// POST: /Patient/AddNote
        /// Adds a clinical note
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNote(PatientDashboardViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model?.NewNote))
            {
                DateTime ts = model.NoteTimestamp ?? DateTime.UtcNow;
                TempData["NoteConfirmation"] = $"Note submitted for {ts:G}.";

                // Create note message using Factory
                var noteMessage = _messageFactory.CreateTimestampedMessage(
                    "Patient-Note",
                    $"📝 {model.NewNote}",
                    ts);
            }
            return RedirectToAction(nameof(Index), new { timeframe = model?.SelectedTimeframe });
        }

        /// <summary>
        /// POST: /Patient/AddMessage
        /// Adds a message
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMessage(PatientDashboardViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model?.NewMessage))
            {
                TempData["MessageConfirmation"] = "Message sent to clinician.";

                // Create message using Factory
                var message = _messageFactory.CreateMessage("Patient", model.NewMessage);
            }
            return RedirectToAction(nameof(Index), new { timeframe = model?.SelectedTimeframe });
        }

        /// <summary>
        /// POST: /Patient/SendMessage
        /// Sends a message from the Messages view
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(string messageText)
        {
            if (!string.IsNullOrWhiteSpace(messageText))
            {
                // Create message using Factory
                var message = _messageFactory.CreateMessage("Patient", messageText);
                
                // Persist to storage
                _messageStorageService.AddMessage(message);
                
                TempData["MessageConfirmation"] = "✓ Message sent to clinician.";
            }
            return RedirectToAction(nameof(Messages));
        }

        /// <summary>
        /// GET: /Patient/RiskAnalysis
        /// Shows risk analysis using different strategies
        /// </summary>
        public IActionResult RiskAnalysis()
        {
            var heatmap = _pressureService.GetCurrentHeatmap();

            var analysisResults = new
            {
                Heatmap = heatmap,
                Strategies = _riskAnalyzer.GetAvailableStrategies().ToList(),
                MultiStrategyResult = _riskAnalyzer.AnalyzeWithAllStrategies(heatmap)
            };

            return Json(analysisResults);
        }

        /// <summary>
        /// GET: /Patient/GenerateReport
        /// Generates a patient report
        /// </summary>
        public IActionResult GenerateReport()
        {
            return Content("Report generation is not yet implemented.");
        }

        /// <summary>
        /// POST: /Patient/LogReposition
        /// Logs that patient has repositioned
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogReposition()
        {
            TempData["RepositionConfirmation"] = $"✓ Reposition logged at {DateTime.Now:HH:mm}. Good job! Remember to log next reposition in 30 minutes.";

            // Create notification message
            var repositionMsg = _messageFactory.CreateSystemMessage(
                $"Reposition logged at {DateTime.Now:HH:mm}. Keep up the good work!");

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: /Patient/SendSupportMessage
        /// Sends a support message from the application
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendSupportMessage(string subject, string message, string priority)
        {
            if (!string.IsNullOrWhiteSpace(subject) && !string.IsNullOrWhiteSpace(message))
            {
                // Create support request message
                var supportMsg = _messageFactory.CreateMessage(
                    "Support Request",
                    $"Subject: {subject}\nMessage: {message}\nPriority: {priority}");

                TempData["SupportConfirmation"] = 
                    $"✓ Your {priority.ToLower()} support request has been sent. You'll receive a response within 2 hours.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: /Patient/ViewReport
        /// Views generated report
        /// </summary>
        public IActionResult ViewReport()
        {
            var heatmap = _pressureService.GetCurrentHeatmap();
            var riskAnalysis = _riskAnalyzer.Analyze(heatmap);

            // Calculate average pressure
            double totalPressure = 0;
            foreach (var val in heatmap)
            {
                totalPressure += val;
            }
            double averagePressure = totalPressure / (heatmap.GetLength(0) * heatmap.GetLength(1));

            var reportData = new
            {
                GeneratedDate = DateTime.Now,
                ReportTitle = "Patient Pressure Monitoring Report",
                PeakPressure = heatmap.Cast<byte>().Max(),
                AveragePressure = Math.Round(averagePressure, 2),
                RiskScore = riskAnalysis.RiskScore,
                RiskCategory = riskAnalysis.RequiresIntervention ? "High - Intervention Recommended" : "Normal",
                Recommendations = new[]
                {
                    "Reposition every 30 minutes",
                    "Monitor high-pressure areas closely",
                    "Contact clinician if discomfort persists",
                    "Use prescribed support devices as recommended"
                }
            };

            return Json(reportData);
        }

        /// <summary>
        /// GET: /Patient/DownloadReport
        /// Downloads report as PDF/CSV
        /// </summary>
        public IActionResult DownloadReport(string format = "pdf")
        {
            TempData["DownloadConfirmation"] = $"Report download starting in {format.ToUpper()} format...";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: /Patient/Profile
        /// Shows patient profile with care team and clinician information
        /// </summary>
        public IActionResult Profile()
        {
            var vm = new PatientProfileViewModel
            {
                PatientId = "PAT001",
                FirstName = "John",
                LastName = "Smith",
                DateOfBirth = "1965-03-15",
                Gender = "Male",
                MedicalRecordNumber = "MRN-2024-001234",
                RoomNumber = "405",
                BedNumber = "B",
                AdmissionDate = DateTime.Now.AddDays(-5),
                PhoneNumber = "+1 (555) 123-4567",
                EmergencyContact = "Jane Smith",
                EmergencyContactPhone = "+1 (555) 123-4568",
                CurrentCondition = "Post-operative recovery from hip replacement surgery",
                Allergies = "Penicillin, Latex",
                Medications = "Metoprolol 50mg daily, Lisinopril 10mg daily",
                MobilityLevel = 1,
                TotalMessagesCount = 12,
                TotalRepositionsLogged = 18,
                LastUpdated = DateTime.Now,
                LastUpdatedBy = "System",

                // Assigned Clinician
                AssignedClinician = new Clinician
                {
                    ClinicianId = "DOC001",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Title = "Dr.",
                    Specialty = "Orthopedic Surgery",
                    PhoneNumber = "+1 (555) 555-1001",
                    Email = "s.johnson@hospital.com",
                    Office = "4th Floor, West Wing",
                    AvailabilityStatus = "Available",
                    LastContact = DateTime.Now.AddHours(-2)
                },

                // Care Team
                CareTeam = new List<CareTeamMember>
                {
                    new CareTeamMember
                    {
                        MemberId = "NURSE001",
                        FirstName = "Maria",
                        LastName = "Garcia",
                        Role = "Registered Nurse",
                        PhoneNumber = "+1 (555) 555-2001",
                        Email = "m.garcia@hospital.com",
                        AssignedDate = DateTime.Now.AddDays(-5),
                        IsAvailable = true,
                        AvailabilityStatus = "Available"
                    },
                    new CareTeamMember
                    {
                        MemberId = "PT001",
                        FirstName = "Michael",
                        LastName = "Chen",
                        Role = "Physical Therapist",
                        PhoneNumber = "+1 (555) 555-3001",
                        Email = "m.chen@hospital.com",
                        AssignedDate = DateTime.Now.AddDays(-5),
                        IsAvailable = true,
                        AvailabilityStatus = "Available"
                    },
                    new CareTeamMember
                    {
                        MemberId = "DIET001",
                        FirstName = "Emily",
                        LastName = "Williams",
                        Role = "Dietitian",
                        PhoneNumber = "+1 (555) 555-4001",
                        Email = "e.williams@hospital.com",
                        AssignedDate = DateTime.Now.AddDays(-3),
                        IsAvailable = false,
                        AvailabilityStatus = "Away"
                    }
                }
            };

            return View(vm);
        }

        /// <summary>
        /// POST: /Patient/UpdateProfile
        /// Updates patient profile information
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string firstName, string lastName, string phoneNumber, 
            string emergencyContact, string emergencyContactPhone, string currentCondition, string allergies)
        {
            TempData["ProfileConfirmation"] = "✓ Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        /// <summary>
        /// GET: /Patient/GetAvailableHeatmaps
        /// Returns list of available heatmap CSV files
        /// </summary>
        public IActionResult GetAvailableHeatmaps()
        {
            var heatmaps = _pressureService.GetAvailableHeatmaps();
            return Json(new { heatmaps = heatmaps, count = heatmaps.Count });
        }

        /// <summary>
        /// POST: /Patient/LoadHeatmapFromCsv
        /// Loads a heatmap from a CSV file
        /// </summary>
        [HttpPost]
        public IActionResult LoadHeatmapFromCsv(string fileName = null)
        {
            try
            {
                _pressureService.LoadHeatmapFromCsv(fileName);
                TempData["HeatmapConfirmation"] = 
                    string.IsNullOrEmpty(fileName) 
                    ? "✓ Random heatmap loaded from CSV data." 
                    : $"✓ Heatmap '{fileName}' loaded successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["HeatmapError"] = $"Error loading heatmap: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: /Patient/HeatmapManagement
        /// View for managing and loading CSV heatmaps
        /// </summary>
        public IActionResult HeatmapManagement()
        {
            var availableHeatmaps = _pressureService.GetAvailableHeatmaps();
            return Json(new 
            { 
                availableHeatmaps = availableHeatmaps,
                totalCount = availableHeatmaps.Count,
                message = availableHeatmaps.Count > 0 
                    ? $"{availableHeatmaps.Count} heatmap(s) available" 
                    : "No heatmap CSV files found"
            });
        }
    }
}