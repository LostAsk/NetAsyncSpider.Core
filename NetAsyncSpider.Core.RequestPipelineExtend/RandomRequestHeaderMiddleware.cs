using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetAsyncSpider.Core.RequestPipeline;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using NetAsyncSpider.Core.Untils;
using System.Linq;
using Exceptionless;
namespace NetAsyncSpider.Core.RequestPipelineExtend
{
    public class RandomRequestHeaderMiddleware : BaseRequestsMiddleware
    {
        private SpiderOptions Options { get; set; }
        /// <summary>
        /// 内置请求头
        /// </summary>
        private List<string> BuiltInArgents = new List<string>() {
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36",
        };
        public override Task InitializeAsync(IServiceProvider serviceProvider)
        {
            Options=serviceProvider.GetService<IOptions<SpiderOptions>>().Value;
            var list = new List<string>();
            ///获取配置中的请求头
            if (Options.Object["Agent"] != null) {
                list = Options.Object["Agent"].Values<string>().ToList();
            }
            BuiltInArgents.AddRange(list);
            return Task.CompletedTask;
        }

        public override Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<IgnoreRequestException, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger)
        {
            ///随机请求头
            var rand = RandomData.GetInt(0, BuiltInArgents.Count - 1);
            request.Headers[HeaderNames.UserAgent] = BuiltInArgents[rand];
            return base.ProcessRequestSetNull();
        }

        public override Task<IRequestParam> ProcessResponseAsync(IRequestParam request, IResponseParam response, BaseSpider spider, ILogger logger)
        {
            Console.WriteLine($"随机请求头:{request.Headers[HeaderNames.UserAgent]}");
            return Task.FromResult((IRequestParam)null);
        }
    }
}
