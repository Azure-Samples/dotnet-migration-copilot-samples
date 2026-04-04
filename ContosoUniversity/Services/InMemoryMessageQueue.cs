using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ContosoUniversity.Services
{
    public class InMemoryMessageQueue : IMessageQueue
    {
        private readonly ILogger<InMemoryMessageQueue> _logger;

        public InMemoryMessageQueue(ILogger<InMemoryMessageQueue> logger)
        {
            _logger = logger;
        }

        public Task SendAsync<T>(T message, MessagePriority priority = MessagePriority.Normal) where T : class
        {
            _logger.LogInformation("InMemoryMessageQueue: Sending message of type {MessageType}", typeof(T).Name);
            return Task.CompletedTask;
        }

        public Task<T?> ReceiveAsync<T>(TimeSpan timeout) where T : class
        {
            return Task.FromResult<T?>(null);
        }

        public bool Exists(string queueName)
        {
            return true;
        }

        public void Create(string queueName)
        {
            _logger.LogInformation("InMemoryMessageQueue: Creating queue {QueueName}", queueName);
        }
    }
}