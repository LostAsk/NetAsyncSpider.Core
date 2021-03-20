using Microsoft.Extensions.Logging;
using NetAsyncSpider.Core.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.ItemPipeline
{
    /// <summary>
    /// 管道处理中间件
    /// </summary>
    public interface IItemPipeline : IDisposable
    {
        /// <summary>
        /// 提供者名称
        /// </summary>
        string ProviderName { get; }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync(IServiceProvider serviceProvider);
        /// <summary>
        /// 管道处理
        /// </summary>
        /// <param name="baseSpider"></param>
        /// <param name="responseParam"></param>
        /// <returns></returns>
        Task HandleAsync(BaseSpider baseSpider, IResponseParam responseParam,ILogger logger, IScheduler scheduler);
    }
}
