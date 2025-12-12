using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SENSORE_APP.Hubs
{
    /// <summary>
    /// SignalR hub for real-time pressure map and metrics updates.
    /// </summary>
    public class PressureHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await Clients.Others.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.Others.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId, message);
        }

        public async Task RequestHeatmapUpdate(byte[,] heatmap)
        {
            // Convert 2D array to jagged array for JSON serialization
            var jagged = new byte[32][];
            for (int i = 0; i < 32; i++)
            {
                jagged[i] = new byte[32];
                for (int j = 0; j < 32; j++)
                {
                    jagged[i][j] = heatmap[i, j];
                }
            }

            await Clients.All.SendAsync("HeatmapUpdated", jagged);
        }
    }
}
