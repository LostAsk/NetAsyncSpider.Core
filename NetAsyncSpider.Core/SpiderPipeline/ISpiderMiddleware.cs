using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.SpiderPipeline
{
    public interface ISpiderMiddleware : IDisposable
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

        Task ProcessSpiderInputAsync(IResponseParam responseParam, BaseSpider baseSpider);

        Task ProcessSpiderOutputAsync(IResponseParam responseParam, BaseSpider baseSpider);

        Task ProcessSpiderExceptionAsync(IResponseParam responseParam,BaseSpider baseSpider, System.Exception exception);
    }
}
