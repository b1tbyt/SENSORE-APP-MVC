using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SENSORE_APP.Services
{
    /// <summary>
    /// Service to load heatmap data from CSV files
    /// CSV files should contain pressure sensor readings in a grid format (32x32)
    /// </summary>
    public class HeatmapCsvLoader
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _dataDirectory;
        private List<string> _availableCsvFiles;
        private Random _random;

        public HeatmapCsvLoader(IWebHostEnvironment environment)
        {
            _environment = environment;
            _dataDirectory = Path.Combine(_environment.ContentRootPath, "Data", "Heatmaps");
            _random = new Random();
            _availableCsvFiles = new List<string>();
            LoadAvailableCsvFiles();
        }

        /// <summary>
        /// Loads all available CSV files from the Data/Heatmaps directory
        /// </summary>
        private void LoadAvailableCsvFiles()
        {
            try
            {
                if (Directory.Exists(_dataDirectory))
                {
                    _availableCsvFiles = Directory.GetFiles(_dataDirectory, "*.csv")
                        .Select(f => Path.GetFileName(f))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading CSV files: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a random heatmap from the available CSV files
        /// </summary>
        public byte[,] GetRandomHeatmap()
        {
            if (_availableCsvFiles.Count == 0)
            {
                return GenerateRandomFallbackHeatmap();
            }

            string randomFile = _availableCsvFiles[_random.Next(_availableCsvFiles.Count)];
            return LoadHeatmapFromCsv(randomFile);
        }

        /// <summary>
        /// Gets a specific heatmap by filename
        /// </summary>
        public byte[,] GetHeatmapByName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return GetRandomHeatmap();
            }

            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
            }

            return LoadHeatmapFromCsv(fileName);
        }

        /// <summary>
        /// Loads heatmap data from a CSV file
        /// Expected format: 32x32 grid of byte values (0-255)
        /// </summary>
        private byte[,] LoadHeatmapFromCsv(string fileName)
        {
            byte[,] heatmap = new byte[32, 32];

            try
            {
                string filePath = Path.Combine(_dataDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    return GenerateRandomFallbackHeatmap();
                }

                string[] lines = File.ReadAllLines(filePath);

                for (int i = 0; i < Math.Min(lines.Length, 32); i++)
                {
                    string[] values = lines[i].Split(',');

                    for (int j = 0; j < Math.Min(values.Length, 32); j++)
                    {
                        if (byte.TryParse(values[j].Trim(), out byte value))
                        {
                            heatmap[i, j] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading heatmap from {fileName}: {ex.Message}");
                return GenerateRandomFallbackHeatmap();
            }

            return heatmap;
        }

        /// <summary>
        /// Generates a random fallback heatmap if no CSV files are available
        /// </summary>
        private byte[,] GenerateRandomFallbackHeatmap()
        {
            byte[,] heatmap = new byte[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    heatmap[i, j] = (byte)_random.Next(256);
                }
            }
            return heatmap;
        }

        /// <summary>
        /// Gets list of all available CSV files
        /// </summary>
        public List<string> GetAvailableHeatmaps()
        {
            return _availableCsvFiles;
        }

        /// <summary>
        /// Reloads the available CSV files list
        /// Call this if new CSV files are added at runtime
        /// </summary>
        public void RefreshAvailableFiles()
        {
            _availableCsvFiles.Clear();
            LoadAvailableCsvFiles();
        }
    }
}
