
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
    /// 测试请求中间件(也可以继承BaseRequestsMiddleware，有一些常用简化方法)
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

        public Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<NetAsyncSpider.Core.Exception, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger)
        {
            Console.WriteLine($"进入请求拦截");
            return Task.FromResult(((IBaseReqParam)null, (Func<IServiceProvider, BaseSpider, IResponseParam, Task>)null,(Func<NetAsyncSpider.Core.Exception, Task>)null));
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
        public override Task PreRequest(IRequestParam requestParam)
        {
            return Task.CompletedTask;
        }


        public override async Task InitializeAsync(CancellationToken stoppingToken = default)
        {
            ///默认使用定义的下载器
            RequestParam.SetDefault(x => x.DownProvider,typeof(TestDownProvider));
            var test1 = new RequestParam($"https://localhost:5001/weatherforecast/1")
            {
                Timeout = 0,
                ///默认使用刚才注册的定义策略
                PolicyBuilderKey = "test",
            };
            test1.SetDownProvider<TestDownProvider>();

            var test = Enumerable.Range(0,2).Select(x => new RequestParam($"https://localhost:5001/weatherforecast/{x}") { 
                Timeout=0,
                ///默认使用刚才注册的定义策略
                PolicyBuilderKey="test",
            }).ToList();
            test[0].Properties.Add(RequestConstProperties.Proxy, "fff");
            foreach (var i in test) {
                ///第一次推送任务用这个方法
                await Scheduler.FirstEnqueueAsync(i, null, null);
            }
        }
    }
}
