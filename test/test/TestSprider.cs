
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetAsyncSpider.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetAsyncSpider.Core.Untils;
using System.Text.RegularExpressions;
using NetAsyncSpider.Core.RequestPipeline;
using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.Scheduler;

namespace test
{
    /// <summary>
    /// 测试请求中间件
    /// </summary>
    public class TestRequestMiddleware : IRequestMiddleware
    {
        public string ProviderName => this.GetType().Name;

        public void Dispose()
        {
            
        }

        public Task InitializeAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"{ProviderName}正在InitializeAsync");
            return Task.CompletedTask;
        }

        public Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<IgnoreRequestException, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger)
        {
            Console.WriteLine($"进入请求拦截");
            return Task.FromResult(((IBaseReqParam)null, (Func<IServiceProvider, BaseSpider, IResponseParam, Task>)null,(Func<IgnoreRequestException, Task>)null));
        }

        public Task<IRequestParam> ProcessResponseAsync(IRequestParam request, IResponseParam response, BaseSpider spider, ILogger logger)
        {
            Console.WriteLine($"进入响应拦截");
            return Task.FromResult((IRequestParam)null);
        }
    }

    /// <summary>
    /// 测试管道中间件
    /// </summary>
    public class TestItemPipeline : BaseItemPipeline, IItemPipeline
    {
        public override Task HandleAsync(BaseSpider baseSpider, IResponseParam responseParam, ILogger logger, IScheduler scheduler)
        {
            var body=Encoding.UTF8.GetString(responseParam.ResponseContent);
            Console.WriteLine($"管道中间件拦截,输出响应body {body}");
            return Task.CompletedTask;
        }

        public override Task InitializeAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine($"{ProviderName}正在InitializeAsync");
            return Task.CompletedTask; ;
        }
    }
    /// <summary>
    /// 测试spider
    /// </summary>
    public class TestSprider : BaseSpider
    {
        public  TestSprider(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        /// <summary>
		/// 主要作用是产生新的requestparam<br></br>
		/// 解析可由中间件解析<br></br>
		/// 或者在这里解析,放到参数IResponseParam,中间件获取解析对象进行操作
		/// </summary>
		/// <param name="responseParam"></param>
		/// <returns></returns> 
        public override async Task ParseAsync(IResponseParam responseParam)
        {
            ///测试递归
            Console.WriteLine($"url :{responseParam.RequestParam.Uri} 第{responseParam.RequestParam.Depth}层节点");
            var x = responseParam.TargetUrl.Split("/")[^1..][0];
            var tmp = int.Parse(x) + 10;
            var url = $"https://localhost:5001/weatherforecast/{tmp}";
            var k = responseParam.RequestParam.CloneSetUri(url);
            k.Method = HeaderNames.Head;
            ///推送任务
            await Scheduler.EnqueueAsync(k, responseParam);
        }
        /// <summary>
		/// 请求预处理<br></br>
		/// 可设置每个请求之前的时间间隔防止太快<br></br>
		/// 由于httpclientfactory要预设置<br></br>
		/// 所以通过设置IRequestParam.ClientKey="xxx"找到对应的httpclient
		/// </summary>
		/// <param name="requestParam"></param>
		/// <returns></returns>
        public override Task PreRequest(IRequestParam requestParam)
        {
            return Task.CompletedTask;
        }


        public override async Task InitializeAsync(CancellationToken stoppingToken = default)
        {
            ///默认超时为0
            RequestParam.SetDefault(x => x.Timeout, 0);
            ///默认使用刚才注册的定义策略
            RequestParam.SetDefault(x => x.PolicyBuilderKey, "test");
            ///默认使用定义的下载器
            RequestParam.SetDefault(x => x.DownProvider,typeof(TestDownProvider));
            var test = Enumerable.Range(0,10).Select(x => new RequestParam($"https://localhost:5001/weatherforecast/{x}") { }).ToList();
            test[0].Properties.Add(RequestConstProperties.Proxy, "fff");//[] = "ffff";
            foreach (var i in test) {
                ///第一次推送任务用这个方法
                await Scheduler.FirstEnqueueAsync(i, null, null);
            }
        }
    }
}
