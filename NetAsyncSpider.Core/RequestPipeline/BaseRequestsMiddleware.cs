using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.RequestPipeline
{
    public abstract class BaseRequestsMiddleware : IRequestMiddleware
    {
        public string ProviderName => this.GetType().Name;

        public virtual void Dispose()
        {
            
        }

        public abstract Task InitializeAsync(IServiceProvider serviceProvider);
        /// <summary>
        /// 设置null的快捷方式
        /// </summary>
        /// <returns></returns>
        protected Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<Exception, Task>)> ProcessRequestSetNull() {
            return Task.FromResult(((IBaseReqParam)null, (Func<IServiceProvider, BaseSpider, IResponseParam, Task>)null, (Func<Exception, Task>) null));
        }
        /// <summary>
        /// 设置null的快捷方式
        /// </summary>
        /// <returns></returns>
        protected Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<Exception, Task>)> ProcessRequestSetNull(IResponseParam responseParam)
        {
            return Task.FromResult((responseParam as IBaseReqParam, (Func<IServiceProvider, BaseSpider, IResponseParam, Task>)null, (Func<Exception, Task>)null));
        }


        public abstract Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<Exception, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger);


        public abstract Task<IRequestParam> ProcessResponseAsync(IRequestParam request, IResponseParam response, BaseSpider spider, ILogger logger);

    }
}
