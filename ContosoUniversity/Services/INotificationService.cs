using ContosoUniversity.Models;

namespace ContosoUniversity.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string entityType, string entityId,
        string? entityDisplayName, EntityOperation operation, string? userName = null);
    Task<Notification?> ReceiveNotificationAsync(CancellationToken cancellationToken = default);
    Task<List<Notification>> GetRecentNotificationsAsync(int count = 50);
}
