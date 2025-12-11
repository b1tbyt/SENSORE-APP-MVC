using System;
using System.Collections.Generic;

namespace SENSORE_APP.Services.Adapters
{
    /// <summary>
    /// Adapter Pattern (Structural)
    /// Adapts incompatible interfaces to work together.
    /// This adapter converts legacy pressure sensor formats to our standardized format.
    /// </summary>

    /// <summary>
    /// Interface for legacy/external pressure sensor data
    /// Represents data from external systems that don't match our format
    /// </summary>
    public interface ILegacyPressureSensor
    {
        /// <summary>
        /// Gets raw sensor readings in legacy format (0-1000 scale)
        /// </summary>
        int[][] GetLegacyReadings();

        /// <summary>
        /// Gets sensor calibration factor
        /// </summary>
        double GetCalibrationFactor();
    }

    /// <summary>
    /// Our standardized pressure sensor interface
    /// </summary>
    public interface IStandardPressureSensor
    {
        /// <summary>
        /// Gets normalized sensor readings (0-255 scale)
        /// </summary>
        byte[,] GetNormalizedReadings();

        /// <summary>
        /// Gets quality score of the reading
        /// </summary>
        double GetQualityScore();
    }

    /// <summary>
    /// Adapter that converts legacy sensor format to standard format
    /// This allows us to use old sensors without changing our business logic
    /// </summary>
    public class LegacyPressureSensorAdapter : IStandardPressureSensor
    {
        private readonly ILegacyPressureSensor _legacySensor;
        private const int LegacyMaxValue = 1000;
        private const int StandardMaxValue = 255;

        public LegacyPressureSensorAdapter(ILegacyPressureSensor legacySensor)
        {
            _legacySensor = legacySensor ?? throw new ArgumentNullException(nameof(legacySensor));
        }

        /// <summary>
        /// Converts legacy readings to standardized 32x32 byte array
        /// </summary>
        public byte[,] GetNormalizedReadings()
        {
            var legacyReadings = _legacySensor.GetLegacyReadings();
            var calibrationFactor = _legacySensor.GetCalibrationFactor();

            // Handle different legacy formats
            int rows = legacyReadings.Length;
            int cols = rows > 0 ? legacyReadings[0].Length : 0;

            // If not 32x32, resize to 32x32
            var normalized = new byte[32, 32];

            if (rows == 32 && cols == 32)
            {
                // Direct conversion for matching dimensions
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        normalized[i, j] = ConvertValue(legacyReadings[i][j], calibrationFactor);
                    }
                }
            }
            else
            {
                // Resample/interpolate if dimensions don't match
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        int sourceRow = (int)((double)i / 32 * rows);
                        int sourceCol = (int)((double)j / 32 * cols);

                        if (sourceRow >= rows) sourceRow = rows - 1;
                        if (sourceCol >= cols) sourceCol = cols - 1;

                        normalized[i, j] = ConvertValue(legacyReadings[sourceRow][sourceCol], calibrationFactor);
                    }
                }
            }

            return normalized;
        }

        /// <summary>
        /// Calculates quality score of the conversion
        /// </summary>
        public double GetQualityScore()
        {
            // Quality is based on calibration factor validity
            var factor = _legacySensor.GetCalibrationFactor();
            return Math.Min(1.0, factor / 1.5); // Normalize to 0-1 range
        }

        /// <summary>
        /// Converts individual pressure value from legacy to standard format
        /// </summary>
        private byte ConvertValue(int legacyValue, double calibrationFactor)
        {
            // Apply calibration and convert from 0-1000 to 0-255
            double calibrated = legacyValue * calibrationFactor;
            double normalized = (calibrated / LegacyMaxValue) * StandardMaxValue;
            return (byte)Math.Max(0, Math.Min(255, normalized));
        }
    }

    /// <summary>
    /// Mock implementation of legacy sensor for testing/demonstration
    /// </summary>
    public class MockLegacyPressureSensor : ILegacyPressureSensor
    {
        private readonly Random _random = new Random();

        public int[][] GetLegacyReadings()
        {
            var readings = new int[24][]; // Legacy format: 24x24 grid
            for (int i = 0; i < 24; i++)
            {
                readings[i] = new int[24];
                for (int j = 0; j < 24; j++)
                {
                    readings[i][j] = _random.Next(0, 1000);
                }
            }
            return readings;
        }

        public double GetCalibrationFactor()
        {
            return 1.2; // Example calibration factor
        }
    }

    /// <summary>
    /// Adapter for third-party sensor APIs
    /// </summary>
    public class ThirdPartySensorAdapter : IStandardPressureSensor
    {
        private readonly IThirdPartySensorAPI _externalAPI;

        public interface IThirdPartySensorAPI
        {
            SensorDataDto GetSensorData();
            double GetAccuracy();
        }

        public class SensorDataDto
        {
            public List<List<int>> Matrix { get; set; }
            public int Resolution { get; set; }
        }

        public ThirdPartySensorAdapter(IThirdPartySensorAPI externalAPI)
        {
            _externalAPI = externalAPI ?? throw new ArgumentNullException(nameof(externalAPI));
        }

        public byte[,] GetNormalizedReadings()
        {
            var data = _externalAPI.GetSensorData();
            var normalized = new byte[32, 32];

            int srcRows = data.Matrix.Count;
            int srcCols = srcRows > 0 ? data.Matrix[0].Count : 0;

            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    int srcI = (srcRows > 0) ? (i * srcRows) / 32 : 0;
                    int srcJ = (srcCols > 0) ? (j * srcCols) / 32 : 0;

                    srcI = Math.Min(srcI, srcRows - 1);
                    srcJ = Math.Min(srcJ, srcCols - 1);

                    int value = data.Matrix[srcI][srcJ];
                    normalized[i, j] = (byte)Math.Min(255, value / 4); // Assuming 0-1000 range
                }
            }

            return normalized;
        }

        public double GetQualityScore()
        {
            return _externalAPI.GetAccuracy();
        }
    }
}
