
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
    /// 测试spider
    /// </summary>
    public class cookietest_spider : BaseSpider
    {
        public cookietest_spider(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public override async Task ParseAsync(IResponseParam responseParam)
        {
            if (!responseParam.RequestParam.UserCookie.Keys.Any()) {
                var url = $"https://localhost:5001/weatherforecast/cookie";
                var k = responseParam.RequestParam.CloneSetUri(url);
                ///推送任务
                await Scheduler.EnqueueAsync(k, responseParam, GetHeader);
            }

            
        }

        private async Task GetHeader(IServiceProvider serviceProvider, BaseSpider baseSpider, IResponseParam responseParam)
        {
            var url = $"https://localhost:5001/weatherforecast/header";
            var k = responseParam.RequestParam.CloneSetUri(url);
            ///推送任务
            await Scheduler.EnqueueAsync(k, responseParam);
        }


        public override Task PreRequest(IRequestParam requestParam)
        {
            return Task.CompletedTask;
        }


        public override async Task InitializeAsync(CancellationToken stoppingToken = default)
        {
            var test1 = new RequestParam($"https://localhost:5001/weatherforecast/header")
            {
                Timeout = 0,
            };
            await Scheduler.FirstEnqueueAsync(test1, null, null);
            
        }
    }
}
