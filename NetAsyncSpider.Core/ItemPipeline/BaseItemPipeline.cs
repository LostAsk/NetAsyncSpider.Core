using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using NetAsyncSpider.Core.Untils;
using NetAsyncSpider.Core.Scheduler;
namespace NetAsyncSpider.Core.ItemPipeline
{
	/// <summary>
	/// 数据流处理器基类
	/// </summary>
	public abstract class BaseItemPipeline : IItemPipeline
	{

		public virtual string ProviderName => this.GetType().Name;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public abstract Task InitializeAsync(IServiceProvider serviceProvider);



		/// <summary>
		/// 是否为空
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected virtual bool IsNullOrEmpty(IResponseParam context)
		{
			return context.ResponseContent==null|| context.ResponseContent.Length==0;
		}

		/// <summary>
		/// 释放
		/// </summary>
		public virtual void Dispose()
		{
		}

		public abstract Task HandleAsync(BaseSpider baseSpider, IResponseParam responseParam, ILogger logger, IScheduler scheduler);
    }
}
