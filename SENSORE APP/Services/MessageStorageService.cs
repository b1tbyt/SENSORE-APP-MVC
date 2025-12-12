using SENSORE_APP.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SENSORE_APP.Services
{
    /// <summary>
    /// Service to manage message persistence using session storage
    /// </summary>
    public class MessageStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string MessagesSessionKey = "PatientMessages";

        public MessageStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get all messages for current patient
        /// </summary>
        public List<Message> GetAllMessages()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return new List<Message>();

            var messagesJson = session.GetString(MessagesSessionKey);
            if (string.IsNullOrEmpty(messagesJson))
                return InitializeWithDefaultMessages();

            try
            {
                return JsonSerializer.Deserialize<List<Message>>(messagesJson) ?? new List<Message>();
            }
            catch
            {
                return InitializeWithDefaultMessages();
            }
        }

        /// <summary>
        /// Add a new message
        /// </summary>
        public void AddMessage(Message message)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return;

            var messages = GetAllMessages();
            messages.Add(message);
            SaveMessages(messages);
        }

        /// <summary>
        /// Clear all messages
        /// </summary>
        public void ClearMessages()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(MessagesSessionKey);
            }
        }

        /// <summary>
        /// Save messages to session
        /// </summary>
        private void SaveMessages(List<Message> messages)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var messagesJson = JsonSerializer.Serialize(messages);
                session.SetString(MessagesSessionKey, messagesJson);
            }
        }

        /// <summary>
        /// Initialize with default messages
        /// </summary>
        private List<Message> InitializeWithDefaultMessages()
        {
            return new List<Message>
            {
                new Message
                {
                    Sender = "Clinician",
                    Text = "How are you feeling today?",
                    Timestamp = System.DateTime.UtcNow.AddMinutes(-60)
                },
                new Message
                {
                    Sender = "Patient",
                    Text = "Good, no discomfort currently.",
                    Timestamp = System.DateTime.UtcNow.AddMinutes(-55)
                },
                new Message
                {
                    Sender = "Clinician",
                    Text = "Remember to shift weight every 30 minutes.",
                    Timestamp = System.DateTime.UtcNow.AddMinutes(-50)
                },
                new Message
                {
                    Sender = "Patient",
                    Text = "I have felt some pressure on my left hip.",
                    Timestamp = System.DateTime.UtcNow.AddMinutes(-45)
                },
                new Message
                {
                    Sender = "Clinician",
                    Text = "Let me know if it persists. Take a break and reposition.",
                    Timestamp = System.DateTime.UtcNow.AddMinutes(-30)
                }
            };
        }
    }
}
