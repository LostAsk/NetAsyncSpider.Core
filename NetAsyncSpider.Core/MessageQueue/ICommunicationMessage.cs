using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.MessageQueue
{
    public interface ICommunicationMessage : ICommunicationMessage<byte[]> { 
    }


    public interface ICommunicationMessage<T>:IDisposable
    {
        /// <summary>
        /// 默认并发数(在消息处理注册前设置)
        /// </summary>
        public int CurrentNum { get; set; }
        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishAsync(string queue, T message);
    
        /// <summary>
        /// 消息处理注册
        /// </summary>
        /// <param name="queuekey"></param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RegisterConsumeAsync(string queuekey,Func<IServiceProvider,T,Task> func,CancellationToken cancellationToken);
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="queue"></param>
        void CloseQueue(string queue);
        /// <summary>
        /// 是否分布式
        /// </summary>
        bool IsDistributed { get; }
    }
}
