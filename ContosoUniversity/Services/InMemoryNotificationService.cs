using System.Threading.Channels;
using ContosoUniversity.Models;

namespace ContosoUniversity.Services;

public class InMemoryNotificationService : INotificationService
{
    private readonly Channel<Notification> _channel =
        Channel.CreateBounded<Notification>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    private readonly List<Notification> _history = [];
    private readonly object _lockObj = new object();

    public async Task SendNotificationAsync(string entityType, string entityId,
        string? entityDisplayName, EntityOperation operation, string? userName = null)
    {
        var notification = new Notification
        {
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation.ToString(),
            Message = $"{entityType} '{entityDisplayName ?? entityId}' was {operation.ToString().ToLower()}d",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userName ?? "System",
            IsRead = false
        };

        lock (_lockObj) { _history.Add(notification); }
        await _channel.Writer.WriteAsync(notification);
    }

    public async Task<Notification?> ReceiveNotificationAsync(CancellationToken ct = default)
    {
        try
        {
            if (await _channel.Reader.WaitToReadAsync(ct))
                return await _channel.Reader.ReadAsync(ct);
        }
        catch (OperationCanceledException) { }
        return null;
    }

    public Task<List<Notification>> GetRecentNotificationsAsync(int count = 50)
    {
        lock (_lockObj)
        {
            return Task.FromResult(_history.TakeLast(count).Reverse().ToList());
        }
    }
}
