using System;
using Microsoft.Extensions.Configuration;
using ContosoUniversity.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ContosoUniversity.Services
{
    public class NotificationService : IDisposable
    {
        private readonly string _queuePath;
        private readonly IMessageQueue _messageQueue;
        private readonly IConfiguration _configuration;

        public NotificationService(IMessageQueue messageQueue, IConfiguration configuration)
        {
            _messageQueue = messageQueue;
            _configuration = configuration;

            // Get queue path from configuration or use default
            _queuePath = _configuration["AppSettings:NotificationQueuePath"] ?? @".\Private$\ContosoUniversityNotifications";

            // Ensure the queue exists
            if (!_messageQueue.Exists(_queuePath))
            {
                _messageQueue.Create(_queuePath);
            }
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string? entityDisplayName, EntityOperation operation, string? userName = null)
        {
            try
            {
                var notification = new Notification
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = operation.ToString(),
                    Message = GenerateMessage(entityType, entityId, entityDisplayName, operation),
                    CreatedAt = DateTime.Now,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                // Send notification via message queue abstraction
                _messageQueue.SendAsync(notification, MessagePriority.Normal).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // Log error but don't break the main operation
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        public Notification? ReceiveNotification()
        {
            try
            {
                var notification = _messageQueue.ReceiveAsync<Notification>(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
                return notification;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to receive notification: {ex.Message}");
                return null;
            }
        }

        public void MarkAsRead(int notificationId)
        {
            // In a real implementation, you might want to store notifications in database as well
            // for persistence and tracking read status
        }

        private string GenerateMessage(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            var displayText = !string.IsNullOrWhiteSpace(entityDisplayName) 
                ? $"{entityType} '{entityDisplayName}'" 
                : $"{entityType} (ID: {entityId})";

            switch (operation)
            {
                case EntityOperation.CREATE:
                    return $"New {displayText} has been created";
                case EntityOperation.UPDATE:
                    return $"{displayText} has been updated";
                case EntityOperation.DELETE:
                    return $"{displayText} has been deleted";
                default:
                    return $"{displayText} operation: {operation}";
            }
        }

        public void Dispose()
        {
            // Message queue is managed by DI container
        }
    }
}
