using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using ContosoUniversity.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ContosoUniversity.Services
{
    public class NotificationService : IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly string _queueName;
        private bool _disposed = false;

        public NotificationService(IConfiguration configuration)
        {
            var fullyQualifiedNamespace = configuration["AzureServiceBus:FullyQualifiedNamespace"]
                ?? throw new InvalidOperationException("AzureServiceBus:FullyQualifiedNamespace configuration is required.");
            _queueName = configuration["AzureServiceBus:QueueName"] ?? "contoso-notifications";

            var credential = new DefaultAzureCredential();
            _client = new ServiceBusClient(fullyQualifiedNamespace, credential);
            _sender = _client.CreateSender(_queueName);
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string? userName = null)
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
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName ?? "System",
                    IsRead = false
                };

                var json = JsonConvert.SerializeObject(notification);
                var message = new ServiceBusMessage(json)
                {
                    ContentType = "application/json"
                };

                _sender.SendMessageAsync(message).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // Log error but don't break the main operation
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        public async Task<IReadOnlyList<Notification>> ReceiveNotificationsAsync(int maxMessages = 10)
        {
            var notifications = new List<Notification>();
            try
            {
                await using var receiver = _client.CreateReceiver(_queueName);
                var receivedMessages = await receiver.ReceiveMessagesAsync(
                    maxMessages,
                    maxWaitTime: TimeSpan.FromSeconds(2));

                foreach (var receivedMessage in receivedMessages)
                {
                    try
                    {
                        var json = receivedMessage.Body.ToString();
                        var notification = JsonConvert.DeserializeObject<Notification>(json);
                        if (notification != null)
                        {
                            notifications.Add(notification);
                        }
                        await receiver.CompleteMessageAsync(receivedMessage);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to deserialize notification: {ex.Message}");
                        await receiver.AbandonMessageAsync(receivedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to receive notifications: {ex.Message}");
            }
            return notifications;
        }

        public void MarkAsRead(int notificationId)
        {
            // Message completion is handled during receive in Azure Service Bus.
        }

        private string GenerateMessage(string entityType, string entityId, string? entityDisplayName, EntityOperation operation)
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
            if (!_disposed)
            {
                _sender?.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _client?.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _disposed = true;
            }
        }
    }
}
