using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace NetAsyncSpider.Core.RequestPipeline
{
    /// <summary>
    /// 下载中间件(全局)
    /// </summary>
    public interface IRequestMiddleware:IDisposable
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
        /// 预处理请求 确保内部已处理异常
        /// </summary>
        /// <param name="request"></param>
        /// <param name="spider"></param>
        /// <returns>null的话会继续处理其他IRequestMiddleware以及向下处理<br></br>
        /// IResponseParam 终止后面的IRequestMiddleware处理，然后处理下个流程<br></br>
        /// IRequestParam 终止后面的IRequestMiddleware处理,放回调度器(一般做转发用的)<br></br>
        /// 注意如果返回IRequestParam,才会用到返回的委托；另外不要与传入的IRequestParam相同(即同一个对象)，否则死循环
        /// </returns>
        Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<IgnoreRequestException, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger);
        /// <summary>
        /// 响应处理(IResponseParam包含错误信息)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="spider"></param>
        /// <returns>null的话会继续处理其他IRequestMiddleware以及向下处理<br></br>
        /// IRequestParam会终止出来并重新放入调度器
        /// </returns>
        Task<IRequestParam> ProcessResponseAsync(IRequestParam request, IResponseParam response, BaseSpider spider,ILogger logger);
    }
}
