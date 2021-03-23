using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetAsyncSpider.Core.HashService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NetAsyncSpider.Core.Scheduler;
using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.PolicyHandler;
using NetAsyncSpider.Core.RequestPipeline;
namespace NetAsyncSpider.Core
{
    /// <summary>
    /// 单个流程操作(建议全局)
    /// </summary>
    public class SingleHandler : ISingleHandler
    {
        protected IServiceProvider ServiceProvider { get; }

        protected IScheduler Scheduler { get; }
        protected ILogger<BaseSpider> Logger { get; }
        protected SpiderOptions SpiderOptions { get; }
        public SingleHandler(IServiceProvider serviceProvider, IScheduler scheduler, IOptions<SpiderOptions> options, ILogger<BaseSpider> logger)
        {
            ServiceProvider = serviceProvider;
            Scheduler = scheduler;
            SpiderOptions = options.Value;
            Logger = logger;
        }

        public event Func<IServiceProvider, System.Exception, IRequestParam, IResponseParam, Task> HandlerFailded;

        /// <summary>
        /// 处理中间件dispose
        /// </summary>
        public void Dispose()
        {


        }







        public async Task<IResponseParam> GetResponseAsync(IRequestParam requestParam)
        {
            var handler = requestParam.ResquestPolicyHanderProvider == null ? ServiceProvider.GetService<IResqustPolicyHander>() : (IResqustPolicyHander)ServiceProvider.GetService(requestParam.ResquestPolicyHanderProvider);
            return await GetResponseAsync(requestParam, handler);
        }

        public async Task<IResponseParam> GetResponseAsync(IRequestParam requestParam, IResqustPolicyHander resqustPolicyHander)
        {
            using (resqustPolicyHander)
            {
                resqustPolicyHander.SetInput(requestParam.Hash, requestParam as RequestParam);
                var reponse = await resqustPolicyHander.ExecuteAndCaptureAsync(requestParam.PolicyBuilderKey, Logger);
                resqustPolicyHander.CrearInputByKey(requestParam.Hash);
                return reponse;
            }

        }
        /// <summary>
        /// 对IResponseParam进行加工
        /// </summary>
        /// <param name="responseParam"></param>
        /// <param name="spider"></param>
        /// <param name="ignore_request_handler"></param>
        /// <returns></returns>
        private async Task<(IRequestParam,bool)> DownloaderPipelinesHandlerAsync(IResponseParam responseParam, BaseSpider spider, ILogger logger, Func<Exception, Task> ignore_request_handler = null) {
            var isthrow = false;
            if (SpiderOptions.DownloaderPipelines.Count == 0) return (null, isthrow);
            try
            {
                var dataflows = ServiceProvider.GetServices<IRequestMiddleware>();
                ///按提供者排序
                var pipe_s = SpiderOptions.DownloaderPipelines.Select(x => dataflows.FirstOrDefault(z => z.ProviderName == x)).Where(x => x != null);
                foreach (var pipe in pipe_s)
                {
                    var request = await pipe.ProcessResponseAsync(responseParam.RequestParam, responseParam, spider, logger);
                    if (request != null) return (request, isthrow);
                }
                return (null, isthrow);
            }
            catch (Exception e)
            {
                isthrow = true;
                if(ignore_request_handler != null)
                await ignore_request_handler(e);

            }
            return (null, isthrow);

        }


        public async Task ExcuteAsync(IRequestParam requestParam, BaseSpider spider,Func<IServiceProvider,BaseSpider, IResponseParam,Task> parseasync=null, Func<Exception, Task> ignore_request_handler = null, IResponseParam responseParam=null)
        {
            IResponseParam response = responseParam;
            try
            {
                if (response == null) {
                    response = await GetResponseAsync(requestParam);
                }


                var down_handler = await DownloaderPipelinesHandlerAsync(response, spider, Logger, ignore_request_handler);
                if (down_handler.Item2) {
                    Logger.LogWarning($"Id:{requestParam.Owner} Url:{requestParam.Uri} 下载中间件出错");
                    return;
                }
                else if ((down_handler.Item1 != null && !down_handler.Item2)) {
                    Logger.LogInformation($"Id:{requestParam.Owner} Url:{requestParam.Uri} 重新放入调度器");
                    ///放入调度器
                    await Scheduler.EnqueueAsync(requestParam, null, parseasync, ignore_request_handler);
                    return;
                }
                response.RequestParam.Properties[RequestConstProperties.Depth] = response.RequestParam.Properties[RequestConstProperties.Depth] + 1;
                var ElapsedMilliseconds = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeMilliseconds() - response.RequestParam.Timestamp);
                response.ElapsedMilliseconds = ElapsedMilliseconds;
                if (parseasync == null)
                {
                    
                    await spider.ParseAsync(response);
                }
                else
                {
                    await parseasync(ServiceProvider, spider, response);
                }
                var dataflows = ServiceProvider.GetServices<IItemPipeline>();
                ///按提供者排序
                var select_flows = SpiderOptions.ItemPipelines.Select(x => dataflows.FirstOrDefault(z => z.ProviderName == x)).Where(x => x != null);
                foreach (var flow in select_flows)
                {
                    await flow.HandleAsync(spider, response, Logger, Scheduler);
                }
                //var list = dataFlowContext.FollowRequests.Where(x => x.Depth <= SpiderOptions.Depth).ToList();
                //await Scheduler?.EnqueueAsync(list);
                Logger.LogDebug($"Id:{response.RequestParam.Owner} Url:{response.RequestParam.Uri} 解析完成");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Id:{requestParam.Owner} Url:{requestParam.Uri} 错误{e.Message}");
                if (HandlerFailded != null) await HandlerFailded(ServiceProvider, e, requestParam, response);
            }
            finally {
                requestParam?.Dispose();
                response?.Dispose();
            }

        }
    }
}
