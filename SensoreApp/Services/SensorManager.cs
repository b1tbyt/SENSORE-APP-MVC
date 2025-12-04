namespace SensoreApp.Services
{
    public class SensorManager
    {
        private static SensorManager? _instance;

        private SensorManager() { }

        public static SensorManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SensorManager();
                return _instance;
            }
        }

        public int GetHeartRate()
        {
            return new Random().Next(60, 100);
        }

        public int GetTemperature()
        {
            return new Random().Next(36, 39);
        }
    }
}
