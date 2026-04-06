using System;
using System.Threading.Tasks;

namespace ContosoUniversity.Services
{
    public interface IMessageQueue
    {
        Task SendAsync<T>(T message, MessagePriority priority = MessagePriority.Normal) where T : class;
        Task<T?> ReceiveAsync<T>(TimeSpan timeout) where T : class;
        bool Exists(string queueName);
        void Create(string queueName);
    }

    public enum MessagePriority
    {
        Lowest = 0,
        VeryLow = 1,
        Low = 2,
        Normal = 3,
        AboveNormal = 4,
        High = 5,
        VeryHigh = 6,
        Highest = 7
    }
}