namespace SensoreApp.Patterns
{
    public class VitalSignMonitor
    {
        private static VitalSignMonitor? _instance;

        public int HeartRate { get; private set; }
        public float Temperature { get; private set; }
        public int Oxygen { get; private set; }

        private VitalSignMonitor() { }

        public static VitalSignMonitor Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new VitalSignMonitor();
                return _instance;
            }
        }

        public void UpdateVitals(int heartRate, float temperature, int oxygen)
        {
            HeartRate = heartRate;
            Temperature = temperature;
            Oxygen = oxygen;
        }
    }
}
