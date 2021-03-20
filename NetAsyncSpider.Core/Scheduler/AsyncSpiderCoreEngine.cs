using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Bert.RateLimiters;
using NetAsyncSpider.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetAsyncSpider.Core.Serialize;
using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.Untils;
using NetAsyncSpider.Core.Scheduler;
using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.RequestPipeline;
using Microsoft.Extensions.DependencyInjection;
using NetAsyncSpider.Core.MessageQueue;

namespace NetAsyncSpider.Core.Scheduler
{
    public class AsyncSpiderCoreEngine : IDisposable
    {


        public bool IsStop { get; private set; }

        public SpiderOptions Options { get; }

        protected BaseSpider BaseSpider { get; private set; }

        protected IScheduler Scheduler { get; private set; }

        protected ICommunicationMessage CommunicationMessage { get; private set; }

		protected ILogger Logger { get; }

		protected ISingleHandler SingleHandler { get; }

		protected IServiceProvider ServiceProvider { get; }
		/// <summary>
		/// 是否分布式爬虫
		/// </summary>
		protected bool IsDistributed => CommunicationMessage.IsDistributed;


		public AsyncSpiderCoreEngine(IOptions<SpiderOptions> options, ILogger<BaseSpider> logger, ISingleHandler singleHandler,IServiceProvider serviceProvider) {
			ServiceProvider = serviceProvider;
			Options = options.Value;
			Logger = logger;
			SingleHandler = singleHandler;
		}



        public async Task InitializeAsync(BaseSpider baseSpider, IScheduler scheduler, ICommunicationMessage communicationMessage, CancellationToken stoppingToken) {
            BaseSpider = baseSpider;
            Scheduler = scheduler;
            CommunicationMessage = communicationMessage;
			await PipelineInitializeAsync(baseSpider);
			Logger.LogInformation($"类:{baseSpider.GetType().Name} Id:{baseSpider.SpiderId.Name} Initialize");
			await Scheduler.InitializeAsync(baseSpider.SpiderId.Id);

		}

		public async Task RunAsync(CancellationToken stoppingToken) {

			var init = BaseSpider.InitializeAsync(stoppingToken);
			var consumer_task = RegisterConsumerAsync(BaseSpider, stoppingToken);
			///辅助线程监听退出
			var listen_task = ListenCancelTokenAsync(BaseSpider, stoppingToken);
			var main_run_task = StartSchedulerAsync(BaseSpider, stoppingToken);
			await Task.WhenAll(init,consumer_task, listen_task, main_run_task);

		}




		/// <summary>
		/// 注册消费事件
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		private Task RegisterConsumerAsync(BaseSpider baseSpider, CancellationToken stoppingToken)
		{
			Logger.LogInformation($"类:{baseSpider.GetType().Name} Id:{baseSpider.SpiderId.Id} 正在注册消费事件");

			Func<IServiceProvider, byte[], Task> Consumer = async (serviceprovider, bytes) =>
			{
				var baserequest = await bytes.DeserializeAsync();
				if (baserequest is IRequestParam request)
				{
					var scheduler = serviceprovider.GetService<IScheduler>();
					var func = scheduler.GetPaseHanderByRequestsHash(request.Hash);
					request.Properties[RequestConstProperties.Timestamp] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
					await SingleHandler.ExcuteAsync(request, baseSpider, func.Item1, func.Item2);
				}
				else if (baserequest is IResponseParam response)
				{
					var scheduler = serviceprovider.GetService<IScheduler>();
					var func = scheduler.GetPaseHanderByRequestsHash(response.RequestParam.Hash);
					response.RequestParam.Properties[RequestConstProperties.Timestamp] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
					await SingleHandler.ExcuteAsync(response.RequestParam, baseSpider, func.Item1, func.Item2, response);
				}

			};
			return CommunicationMessage.RegisterConsumeAsync(baseSpider.SpiderId.Id, Consumer, stoppingToken);
		}

		/// <summary>
		/// 中间件处理InitializeAsync
		/// </summary>
		private async Task PipelineInitializeAsync(BaseSpider baseSpider)
		{
			Logger.LogInformation($"类:{baseSpider.GetType().Name} Id:{baseSpider.SpiderId.Id} 中间件初始化");
			var dataflows = ServiceProvider.GetServices<IItemPipeline>();
			foreach (var flow in dataflows)
			{
				await flow.InitializeAsync(ServiceProvider);
			}
			var items = ServiceProvider.GetServices<IRequestMiddleware>();
			foreach (var flow in items)
			{
				await flow.InitializeAsync(ServiceProvider);
			}
		}

		/// <summary>
		/// 调度函数
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		private async Task StartSchedulerAsync(BaseSpider baseSpider, CancellationToken stoppingToken)
		{
			Logger.LogInformation($"类:{baseSpider.GetType().Name} Id:{baseSpider.SpiderId} 开始调度运行");

			await foreach (var request in GetEnumeratorAsync(stoppingToken))
			{
				///跳过不处理的请求
				if (!baseSpider.IsValidRequest(request))
				{
					continue;
				}
				await baseSpider.PreRequest(request);
				await RequestMidHandlerAsync(request, baseSpider, Logger);
			}
		}



		/// <summary>
		/// 中间件处理和推送消息
		/// </summary>
		/// <param name="request"></param>
		/// <param name="spider"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		private async Task RequestMidHandlerAsync(IRequestParam request, BaseSpider spider, ILogger logger)
		{
			if (Options.DownloaderPipelines.Count == 0)
			{
				///推送消息
				await PublishMessagesAsync(spider,request);
				return;
			}
			var dataflows = ServiceProvider.GetServices<IRequestMiddleware>();
			///按提供者排序
			var pipe_s = Options.DownloaderPipelines.Select(x => dataflows.FirstOrDefault(z => z.ProviderName == x)).Where(x => x != null);
			try
			{

				foreach (var pipe in pipe_s)
				{
					var result = await pipe.ProcessRequestAsync(request, spider, logger);
					if (result.Item1 != null && result.Item1 is IResponseParam response)
					{
						response.RequestParam = request;
						///推送消息
						await PublishMessagesAsync(spider,response);
					}
					else if (result.Item1 != null && result.Item1 is IRequestParam request_n)
					{
						await Scheduler.EnqueueAsync(request_n, null, result.Item2, result.Item3);
					}
				}
				///推送消息
				await PublishMessagesAsync(spider,request);
			}
			catch (Exception e)
			{
				Logger.LogError($"Id:{request.Owner} Url:{request.Uri} 请求中间件 ProcessRequestAsync 错误{e.Message} 直接忽略该request");
			}

		}


		/// <summary>
		/// 推送消息
		/// </summary>
		/// <param name="requests"></param>
		/// <returns></returns>
		private async Task PublishMessagesAsync(BaseSpider baseSpider,IBaseReqParam request)
		{
			await CommunicationMessage.PublishAsBytesAsync(baseSpider.SpiderId.Id, request);
		}

		/// <summary>
		/// 注册监听
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		private async Task ListenCancelTokenAsync(BaseSpider baseSpider,CancellationToken stoppingToken)
		{
			Logger.LogInformation($"类:{baseSpider.GetType().Name} Id:{baseSpider.SpiderId.Name} 开启监听 调度器没队列 事件");
			///轮询时间间隔
			var sleepTimeLimit = Options.EmptySleepTime * 1000;
			var timeout = 0;
			while (!stoppingToken.IsCancellationRequested)
			{
				var tmp_time=Scheduler.GetTotal();
				timeout += tmp_time==0? 300:-timeout;
				if (timeout > sleepTimeLimit)
				{
					IsStop = true;
					CommunicationMessage.CloseQueue(baseSpider.SpiderId.Id);
					return;
				}
				await Task.Delay(300);
			}
		}

		private async IAsyncEnumerable<IRequestParam> GetEnumeratorAsync(CancellationToken stoppingToken)
		{

			var bucket = CreateBucket(Options.Speed);
			var batch = (int)Options.Batch;
			var delay = (int)Options.Delay * 1000;
			while (!(!stoppingToken.IsCancellationRequested ^ !IsStop))
			{
				var requests = await Scheduler.DequeueAsync(batch);
				foreach (var request in requests)
				{
					while (bucket.ShouldThrottle(1, out var waitTimeMillis))
					{
						await Task.Delay(waitTimeMillis, default);
					}
					yield return request;
				}
				await Task.Delay(delay, stoppingToken);
			}
		}

		/// <summary>
		///  创建水桶限流模型
		/// </summary>
		/// <param name="speed"></param>
		/// <returns></returns>
		private static FixedTokenBucket CreateBucket(double speed)
		{
			var defaultTimeUnit = speed >= 1 ? (int)(1000 / speed) : (int)((1 / speed) * 1000);
			return new FixedTokenBucket(1, 1, defaultTimeUnit);
		}
		public void Dispose()
        {
			
			CommunicationMessage.Dispose();
			SingleHandler.Dispose();
			ObjectUtilities.DisposeSafely(Logger);
			Scheduler.Dispose();
			try
			{
				var items = ServiceProvider.GetServices<IItemPipeline>();
				var downs = ServiceProvider.GetServices<IRequestMiddleware>();
				foreach (var item in items)
				{
					item?.Dispose();
				}
				foreach (var item in downs)
				{
					item?.Dispose();
				}
			}
			catch { 
			}

		}
    }
}
