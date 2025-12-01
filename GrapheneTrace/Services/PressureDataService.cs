
namespace GrapheneTrace.Services
{
    public class PressureDataService
    {
        private byte[,] _currentHeatmap;
        private byte[,] _targetHeatmap;
        private readonly Random _random = new Random();
        private readonly object _lock = new object();

        public event Action<byte[,]>? OnHeatmapUpdated;

        public PressureDataService()
        {
            _currentHeatmap = new byte[32, 32];
            _targetHeatmap = new byte[32, 32];
            GenerateRandomHeatmap(_targetHeatmap);
        }

        
        public void StartLiveUpdates(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Update target heatmap (simulating sensor input) every 1000ms
                        if (_random.Next(0, 100) < 30) // 30% chance to update target
                        {
                            GenerateRandomHeatmap(_targetHeatmap);
                        }

                        // Smoothly interpolate between current and target
                        InterpolateHeatmap();

                        // Notify subscribers
                        OnHeatmapUpdated?.Invoke(GetHeatmapCopy());

                        // Update every 100ms for smooth animation
                        await Task.Delay(100, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, cancellationToken);
        }
               
        public byte[,] GetCurrentHeatmap()
        {
            lock (_lock)
            {
                return GetHeatmapCopy();
            }
        }

        private void GenerateRandomHeatmap(byte[,] heatmap)
        {
            lock (_lock)
            {
                // Clear the heatmap first
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        heatmap[i, j] = 0;
                    }
                }

                // Create hotspots (areas of higher pressure)
                int numHotspots = _random.Next(2, 4);
                for (int h = 0; h < numHotspots; h++)
                {
                    int centerX = _random.Next(0, 32);
                    int centerY = _random.Next(0, 32);
                    int radius = _random.Next(3, 8);

                    for (int i = 0; i < 32; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            int dist = (int)Math.Sqrt((i - centerX) * (i - centerX) + (j - centerY) * (j - centerY));
                            if (dist <= radius)
                            {
                                byte value = (byte)Math.Max(0, 255 - (dist * 255 / (radius + 1)));
                                heatmap[i, j] = Math.Max(heatmap[i, j], value);
                            }
                        }
                    }
                }

                // Add some base noise
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        if (_random.Next(0, 100) < 15)
                        {
                            heatmap[i, j] = (byte)Math.Max(heatmap[i, j], _random.Next(20, 80));
                        }
                    }
                }
            }
        }

        private void InterpolateHeatmap()
        {
            lock (_lock)
            {
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        // Smooth interpolation: move 25% towards target each frame
                        double current = _currentHeatmap[i, j];
                        double target = _targetHeatmap[i, j];
                        double newValue = current + (target - current) * 0.25;
                        _currentHeatmap[i, j] = (byte)Math.Max(0, Math.Min(255, newValue));
                    }
                }
            }
        }

        private byte[,] GetHeatmapCopy()
        {
            var copy = new byte[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    copy[i, j] = _currentHeatmap[i, j];
                }
            }
            return copy;
        }
    }
}
