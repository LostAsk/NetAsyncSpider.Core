using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.PolicyHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core
{
    public interface ISingleHandler:IDisposable
    {
        /// <summary>
        /// 整个流程处理失败事件
        /// </summary>
        event Func<IServiceProvider, System.Exception,IRequestParam, IResponseParam, Task> HandlerFailded;
        /// <summary>
        /// 整个流程处理
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        Task ExcuteAsync(IRequestParam requestParam,BaseSpider spider,Func<IServiceProvider,BaseSpider,IResponseParam,Task> parseasync=null, Func<Exception, Task> ignore_request_handler = null, IResponseParam responseParam=null);
        /// <summary>
        /// 获取响应(方便一次请求等等)
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        Task<IResponseParam> GetResponseAsync(IRequestParam requestParam);
        /// <summary>
        /// 获取响应(方便一次请求等等)
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        Task<IResponseParam> GetResponseAsync(IRequestParam requestParam, IResqustPolicyHander resqustPolicyHander);
    }
}
