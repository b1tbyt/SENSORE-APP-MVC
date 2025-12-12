using SENSORE_APP.Models;
using System;
using System.Collections.Generic;

namespace SENSORE_APP.Services.Factories
{
    /// <summary>
    /// Factory Pattern (Creational)
    /// Provides a unified interface for creating different types of messages
    /// without exposing the creation logic to the client.
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// Creates a message based on sender type
        /// </summary>
        Message CreateMessage(string sender, string text);

        /// <summary>
        /// Creates a system message (for alerts, notifications)
        /// </summary>
        Message CreateSystemMessage(string text);

        /// <summary>
        /// Creates a timestamped message with custom timestamp
        /// </summary>
        Message CreateTimestampedMessage(string sender, string text, DateTime timestamp);
    }

    /// <summary>
    /// Concrete Factory implementation for creating Message instances
    /// </summary>
    public class MessageFactory : IMessageFactory
    {
        /// <summary>
        /// Creates a standard message with current timestamp
        /// </summary>
        public Message CreateMessage(string sender, string text)
        {
            if (string.IsNullOrWhiteSpace(sender))
                throw new ArgumentException("Sender cannot be empty", nameof(sender));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Message text cannot be empty", nameof(text));

            return new Message
            {
                Sender = sender,
                Text = text,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a system message (notifications, alerts)
        /// </summary>
        public Message CreateSystemMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Message text cannot be empty", nameof(text));

            return new Message
            {
                Sender = "System",
                Text = text,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a message with custom timestamp for historical messages
        /// </summary>
        public Message CreateTimestampedMessage(string sender, string text, DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(sender))
                throw new ArgumentException("Sender cannot be empty", nameof(sender));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Message text cannot be empty", nameof(text));

            return new Message
            {
                Sender = sender,
                Text = text,
                Timestamp = timestamp
            };
        }
    }

    /// <summary>
    /// Concrete Factory for creating specialized medical alert messages
    /// </summary>
    public class AlertMessageFactory : IMessageFactory
    {
        /// <summary>
        /// Creates a standard message (inherits from base behavior)
        /// </summary>
        public Message CreateMessage(string sender, string text)
        {
            return new Message
            {
                Sender = sender,
                Text = text,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a high-priority alert message
        /// </summary>
        public Message CreateSystemMessage(string text)
        {
            return new Message
            {
                Sender = "ALERT",
                Text = $"?? {text}",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates timestamped alert message
        /// </summary>
        public Message CreateTimestampedMessage(string sender, string text, DateTime timestamp)
        {
            return new Message
            {
                Sender = $"ALERT-{sender}",
                Text = $"?? {text}",
                Timestamp = timestamp
            };
        }

        /// <summary>
        /// Creates a critical pressure alert
        /// </summary>
        public Message CreatePressureAlert(double pressureValue)
        {
            return new Message
            {
                Sender = "PRESSURE_ALERT",
                Text = $"?? Critical pressure detected: {pressureValue:F1} (Alert level)",
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a reposition reminder alert
        /// </summary>
        public Message CreateRepositionAlert(int minutesOverdue)
        {
            return new Message
            {
                Sender = "REPOSITION_ALERT",
                Text = $"? Reposition reminder: {minutesOverdue} minutes overdue",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
