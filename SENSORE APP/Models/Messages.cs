using System;

namespace SENSORE_APP.Models
{
    /// <summary>
    /// Represents a message exchanged between the patient and clinician.
    /// </summary>
    public class Message
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}