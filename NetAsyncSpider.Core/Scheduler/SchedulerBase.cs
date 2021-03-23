using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NetAsyncSpider.Core;
using NetAsyncSpider.Core.HashService;
using NetAsyncSpider.Core.Scheduler.Component;
using NetAsyncSpider.Core.Untils;
using System.Text;
using NetAsyncSpider.Core.Serialize;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Bert.RateLimiters;
using System.Linq;

namespace NetAsyncSpider.Core.Scheduler
{
	public abstract class SchedulerBase : IScheduler
	{

		protected ConcurrentDictionary<string, Func<IServiceProvider, BaseSpider, IResponseParam, Task>> ParseHandler { get; } = new ConcurrentDictionary<string, Func<IServiceProvider, BaseSpider, IResponseParam, Task>>();

		protected ConcurrentDictionary<string, Func<Exception,Task>> IgnoreRequestHandler { get; } = new ConcurrentDictionary<string, Func<Exception, Task>>();

        private SpinLock _spinLock;
		private readonly IHashAlgorithmService _requestHasher;

		protected readonly IDuplicateRemover DuplicateRemover;

		protected SpiderOptions SpiderOption { get; }

		protected ILogger<BaseSpider> Logger { get; }
		protected SchedulerBase(IDuplicateRemover duplicateRemover, IHashAlgorithmService requestHasher,IOptions<SpiderOptions> options, ILogger<BaseSpider> logger)
		{
			DuplicateRemover = duplicateRemover;
			_requestHasher = requestHasher;
			SpiderOption = options.Value;
			Logger = logger;
		}

		/// <summary>
		/// 重置去重器
		/// </summary>
		public virtual async Task ResetDuplicateCheckAsync()
		{
			await DuplicateRemover.ResetDuplicateCheckAsync();
		}

		public virtual Task SuccessAsync(IRequestParam request)
		{
			return Task.CompletedTask;
		}

		public virtual Task FailAsync(IRequestParam request)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// 如果请求未重复就添加到队列中
		/// </summary>
		/// <param name="request">请求</param>
		protected abstract Task PushWhenNoDuplicate(IRequestParam request, IResponseParam responseParam);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			DuplicateRemover.Dispose();
		}




		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		protected abstract Task<IRequestParam[]> ImplDequeueAsync(int count = 1);

		public virtual async Task InitializeAsync(string spiderId)
		{
			await DuplicateRemover.InitializeAsync(spiderId);
		}
		/// <summary>
		/// 队列中的总请求个数
		/// </summary>
		public abstract long GetTotal();

		public async Task<IRequestParam[]> DequeueAsync(int count = 1)
		{
			var locker = false;

			try
			{
				_spinLock.Enter(ref locker);

				return await ImplDequeueAsync(count);
			}
			finally
			{
				
				if (locker)
				{
					_spinLock.Exit();
				}
			}
		}
		public async Task EnqueueAsync(IRequestParam request, IResponseParam responseParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task> parse_handler = null, Func<Exception, Task> ignore_handler = null)
		{

			var encoding = Encoding.GetEncoding(request.Encoding);
			var bytes = new
			{
				request.Owner,
				request.Uri,
				request.Method,
				request.RequestedTimes,
			}.Serialize();
			request.Properties[RequestConstProperties.Hash] = Convert.ToBase64String(_requestHasher.ComputeHash(bytes));
			if (!ParseHandler.ContainsKey(request.Hash))
			{
				ParseHandler.TryAdd(request.Hash, parse_handler);
			}
			if (!IgnoreRequestHandler.ContainsKey(request.Hash))
			{
				IgnoreRequestHandler.TryAdd(request.Hash, ignore_handler);
			}
			var is_disctinct = await DuplicateRemover.IsDuplicateAsync(request);
			if (is_disctinct)
			{
				return;
			}
			await PushWhenNoDuplicate(request, responseParam);

		}

		public async Task FirstEnqueueAsync(IRequestParam requestParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task> parse_handler = null, Func<Exception, Task> ignore_handler = null)
		{
			 requestParam.Properties[RequestConstProperties.Depth] = 0;
			 await EnqueueAsync(requestParam, null,parse_handler,ignore_handler);
		}

        public (Func<IServiceProvider, BaseSpider, IResponseParam, Task>,Func<Exception, Task>) GetPaseHanderByRequestsHash(string resquest_hash)
        {
			Func<IServiceProvider, BaseSpider, IResponseParam, Task> item1 = null;
			ParseHandler.TryGetValue(resquest_hash, out item1);
			Func<Exception, Task> item2 = null;
			IgnoreRequestHandler.TryGetValue(resquest_hash,out item2);
			return (item1, item2);
        }


	}
}
