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
    public class RandomRequestMiddleware : BaseRequestsMiddleware
    {
        private SpiderOptions Options { get; set; }



        private List<string> BuiltInArgents = new List<string>() { 
            "eee","ffff","ggg1233",
        };
        public override Task InitializeAsync(IServiceProvider serviceProvider)
        {
            Options=serviceProvider.GetService<IOptions<SpiderOptions>>().Value;
            var list = new List<string>();
            if (Options.Object["Agent"] != null) {
                list = Options.Object["Agent"].Values<string>().ToList();
            }
  
           
            BuiltInArgents.AddRange(list);
            return Task.CompletedTask;
        }

        public override Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<IgnoreRequestException, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger)
        {
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
