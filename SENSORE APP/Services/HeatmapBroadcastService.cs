using Microsoft.AspNetCore.SignalR;
using SENSORE_APP.Hubs;
using System;
using System.Threading.Tasks;

namespace SENSORE_APP.Services
{
    /// <summary>
    /// Service to broadcast heatmap and metrics updates to connected SignalR clients.
    /// </summary>
    public class HeatmapBroadcastService
    {
        private readonly IHubContext<PressureHub> _hubContext;
        private readonly PressureDataService _pressureService;
        private double _previousPeakPressure;
        private double _previousContactArea;

        public HeatmapBroadcastService(IHubContext<PressureHub> hubContext, PressureDataService pressureService)
        {
            _hubContext = hubContext;
            _pressureService = pressureService;

            // Subscribe to pressure service updates and broadcast to all clients
            _pressureService.OnHeatmapUpdated += BroadcastUpdates;
        }

        private async void BroadcastUpdates(byte[,] heatmap)
        {
            try
            {
                // Convert to jagged array for JSON serialization
                var jagged = new byte[32][];
                for (int i = 0; i < 32; i++)
                {
                    jagged[i] = new byte[32];
                    for (int j = 0; j < 32; j++)
                    {
                        jagged[i][j] = heatmap[i, j];
                    }
                }

                // Broadcast heatmap to all connected clients
                await _hubContext.Clients.All.SendAsync("HeatmapUpdated", jagged);

                // Calculate and broadcast metrics
                byte max = 0;
                int contactCells = 0;
                foreach (byte val in heatmap)
                {
                    if (val > max) max = val;
                    if (val >= 50) contactCells++;
                }

                double currentPeak = max;
                double currentContact = Math.Round(contactCells * 100.0 / (32 * 32), 1);
                double avgPeak = (currentPeak + 10) / 50.0 * 10; // Normalize to 0-10 scale

                // Calculate trends
                double peakTrend = currentPeak - _previousPeakPressure;
                double contactTrend = currentContact - _previousContactArea;

                // Broadcast metrics
                var metrics = new
                {
                    peakPressure = currentPeak,
                    contactArea = currentContact,
                    riskScore = Math.Min(10, Math.Max(0, avgPeak)),
                    peakTrend = peakTrend,
                    contactTrend = contactTrend
                };

                await _hubContext.Clients.All.SendAsync("MetricsUpdated", metrics);

                // Update previous values for next trend calculation
                _previousPeakPressure = currentPeak;
                _previousContactArea = currentContact;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error broadcasting updates: {ex.Message}");
            }
        }
    }
}
