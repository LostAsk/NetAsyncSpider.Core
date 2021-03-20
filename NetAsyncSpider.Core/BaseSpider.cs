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

namespace NetAsyncSpider.Core
{
	
	public readonly struct SpiderIdName
	{
		public readonly string Id;
		public readonly string Name;

		public SpiderIdName(string id, string name)
		{
			Id = id??Guid.NewGuid().ToString();
			Name = name;
		}

		public override string ToString()
		{
			return Id+"_"+Name;
		}
	}
	public abstract class BaseSpider : BackgroundService,IDisposable
	{

		protected readonly DependenceServices _services;

		public SpiderOptions Options { get; }

		/// <summary>
		/// 爬虫标识
		/// </summary>
		public SpiderIdName SpiderId { get; private set; }

		/// <summary>
		/// 日志接口
		/// </summary>
		protected ILogger Logger { get; }
		/// <summary>
		/// 调度
		/// </summary>
		protected IScheduler Scheduler { get; }

		protected IServiceProvider ServiceProvider { get; }

		private SimpleEngine SimpleEngine { get; }

		private ICommunicationMessage Message { get; }

		protected BaseSpider(IServiceProvider serviceProvider)
		{

			ServiceProvider = serviceProvider;


			Logger = ServiceProvider.GetService<ILogger<BaseSpider>>();
			Options = ServiceProvider.GetService<IOptions<SpiderOptions>>().Value;
			_services = ServiceProvider.GetService<DependenceServices>(); ;
			Scheduler = ServiceProvider.GetService<IScheduler>(); 
			SimpleEngine = ServiceProvider.GetService<SimpleEngine>();
			Message = ServiceProvider.GetService<ICommunicationMessage>();
		}

		/// <summary>
		/// 初始化爬虫数据
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		public abstract Task InitializeAsync(CancellationToken stoppingToken = default);
		/// <summary>
		/// 主要作用是产生新的requestparam<br></br>
		/// 解析可由中间件解析<br></br>
		/// 或者在这里解析,放到参数IResponseParam,中间件获取解析对象进行操作
		/// </summary>
		/// <param name="responseParam"></param>
		/// <returns></returns>
		public abstract Task ParseAsync(IResponseParam responseParam);
		/// <summary>
		/// 获取爬虫标识和名称
		/// </summary>
		/// <returns></returns>
		protected virtual SpiderIdName GenerateSpiderId()
		{
			var id = _services.Configuration["SPIDER_ID"] ;
			var name = _services.Configuration["SPIDER_NAME"];
			return new SpiderIdName(id, name);
		}


		public override Task StopAsync(CancellationToken cancellationToken)
		{
			Logger.LogInformation($"类:{this.GetType().Name} Id:{SpiderId.Id} 完成啦");
			SimpleEngine.Dispose();
			return Task.CompletedTask;
		}

		/// <summary>
		/// 基类输出
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			
			SpiderId = GenerateSpiderId();
			///指定SpiderId
			RequestParam.InternalSetDefault(x => x.Owner, SpiderId.Id);
		
			await SimpleEngine.InitializeAsync(this, Scheduler, Message, stoppingToken);
			//await InitializeAsync(stoppingToken);
			await SimpleEngine.RunAsync(stoppingToken);



			_services.ApplicationLifetime.StopApplication();
		}



		/// <summary>
		/// 跳过某些请求规则
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public virtual bool IsValidRequest(IRequestParam request)
		{
			return true;
		}

		/// <summary>
		/// 请求预处理<br></br>
		/// 可设置每个请求之前的时间间隔防止太快<br></br>
		/// 由于httpclientfactory要预设置<br></br>
		/// 所以通过设置IRequestParam.ClientKey="xxx"找到对应的httpclient
		/// </summary>
		/// <param name="requestParam"></param>
		/// <returns></returns>
		public virtual async Task PreRequest(IRequestParam requestParam) {
			await Task.Delay(1000);
		}
		


		public override void Dispose()
		{

            GC.Collect();
        }
	}
}
