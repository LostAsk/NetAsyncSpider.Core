
using NetAsyncSpider.Core.MessageQueue;
using System.Threading.Tasks;


namespace NetAsyncSpider.Core.Serialize
{
    public static class MessageQueueExtensions
    {
        public static async Task PublishAsBytesAsync<T>(this ICommunicationMessage messageQueue, string queue, T message)
        {
            var bytes = message.Serialize();
            await messageQueue.PublishAsync(queue, bytes);
        }
    }
}
