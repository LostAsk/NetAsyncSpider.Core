using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
namespace NetAsyncSpider.Core.MessageQueue
{
	/// <summary>
	/// 动态并发模型
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Pipeline<T>
	{
		protected IServiceProvider ServiceProvider { get; }

		private int CurrentNum { get; set; } = 1;

		private Func<IServiceProvider, T, Task> ConsumeActionBlock { get; set; }
		public Pipeline(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		private IPropagatorBlock<T, T> SendBlock { get; set; }

		private IPropagatorBlock<T, T> CreateBufferBlock()
		{
			var bufferBlock = new BufferBlock<T>();
			var actionBlock = new ActionBlock<T>(async x =>
			{
				await ConsumeActionBlock(ServiceProvider, x);
			}, new ExecutionDataflowBlockOptions
			{
				MaxDegreeOfParallelism = CurrentNum
			});
			actionBlock.Completion.ContinueWith(_ =>
				bufferBlock.Complete()
			);
			return DataflowBlock.Encapsulate(actionBlock, bufferBlock);
		}

		public void SendMessage(T Message)
		{
			SendBlock.Post(Message);
		}

		public void CompleteTask()
		{
			SendBlock.Complete();
		}

		public Task Build(Func<IServiceProvider,T,Task> Consume_ActionBlock, int current_count=10)
		{
			CurrentNum = current_count;
			ConsumeActionBlock = Consume_ActionBlock;
			SendBlock = CreateBufferBlock();
			SendBlock.Completion.ContinueWith(_ => {
				Console.WriteLine("消息信号已Complete");
			});
			return SendBlock.Completion;
		}
	}








	public abstract class BaseCommunicationMessage<T> : ICommunicationMessage<T>
    {
		private ConcurrentDictionary<string, Pipeline<T>> PipelineDic { get; }=
		new ConcurrentDictionary<string, Pipeline<T>>();
		protected IServiceProvider ServiceProvider { get; }

		protected string QueueKey { get;  set; }
		/// <summary>
		/// 默认并发数
		/// </summary>
		public int CurrentNum { get; set; }= 20;
		protected BaseCommunicationMessage(IServiceProvider serviceProvider) {
			ServiceProvider = serviceProvider;
            
		}

		public virtual bool IsDistributed => false;
        public virtual void CloseQueue(string queue)
        {
			PipelineDic[queue].CompleteTask();
			
		}

        public virtual void Dispose()
        {
			foreach (var kv in PipelineDic) {
				kv.Value?.CompleteTask();
			}
        }

        public virtual Task PublishAsync(string queue, T message)
        {
			var p = PipelineDic.TryGetValue(queue,out var s);
			if (p == false) throw new System.Exception($"队列标识{queue} 先注册RegisterConsumeAsync");
			s.SendMessage(message);
			return Task.CompletedTask;
		}

        public virtual Task RegisterConsumeAsync(string queue,Func<IServiceProvider, T, Task> func, CancellationToken cancellationToken)
        {
			var p = PipelineDic.GetOrAdd(queue, (x) => ServiceProvider.GetService<Pipeline<T>>());
			return p.Build(func, CurrentNum);
		}
    }
}
