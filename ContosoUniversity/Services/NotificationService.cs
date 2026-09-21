using System;
using System.Configuration;
using System.IO;
// Technology decision (03.04): MSMQ.Messaging is a drop-in replacement for System.Messaging on
// net10.0 - identical API surface/class names under a different namespace - so it was chosen over
// building a custom Channels/Sqlite-backed queue. This still requires the MSMQ Windows feature to
// be installed/running on the host (Get-Service -Name MSMQ); confirmed present in this environment,
// but flagging it since a deployment target without MSMQ would need a follow-up decision.
using MSMQ.Messaging;
using ContosoUniversity.Models;
using Newtonsoft.Json;

namespace ContosoUniversity.Services
{
    public class NotificationService
    {
        private readonly string _queuePath;
        private readonly MessageQueue _queue;

        public NotificationService()
        {
            // Get queue path from configuration or use default
            _queuePath = GetQueuePath() ?? @".\Private$\ContosoUniversityNotifications";
            
            // Ensure the queue exists
            if (!MessageQueue.Exists(_queuePath))
            {
                _queue = MessageQueue.Create(_queuePath);
                _queue.SetPermissions("Everyone", MessageQueueAccessRights.FullControl);
            }
            else
            {
                _queue = new MessageQueue(_queuePath);
            }
            
            // Configure queue formatter
            _queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
        }

        // Reads "legacy.config" explicitly (same approach as SchoolContextFactory, from 03.03) rather
        // than relying on ConfigurationManager.AppSettings' implicit .exe/.dll.config discovery, which
        // varies depending on how the app is launched.
        private static string GetQueuePath()
        {
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(AppContext.BaseDirectory, "legacy.config")
            };
            var config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return config.AppSettings.Settings["NotificationQueuePath"]?.Value;
        }

        public void SendNotification(string entityType, string entityId, EntityOperation operation, string userName = null)
        {
            SendNotification(entityType, entityId, null, operation, userName);
        }

        public void SendNotification(string entityType, string entityId, string entityDisplayName, EntityOperation operation, string userName = null)
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

                var jsonMessage = JsonConvert.SerializeObject(notification);
                var message = new Message(jsonMessage)
                {
                    Label = $"{entityType} {operation}",
                    Priority = MessagePriority.Normal
                };

                _queue.Send(message);
            }
            catch (Exception ex)
            {
                // Log error but don't break the main operation
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        public Notification ReceiveNotification()
        {
            try
            {
                var message = _queue.Receive(TimeSpan.FromSeconds(1));
                var jsonContent = message.Body.ToString();
                return JsonConvert.DeserializeObject<Notification>(jsonContent);
            }
            catch (MessageQueueException ex) when (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
            {
                // No messages available
                return null;
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
            _queue?.Dispose();
        }
    }
}
