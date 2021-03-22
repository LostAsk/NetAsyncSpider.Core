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
using System.Collections.Concurrent;
namespace NetAsyncSpider.Core.RequestPipelineExtend
{
    public class CookieRequestMiddleware : BaseRequestsMiddleware
    {
        private ConcurrentDictionary<string, Dictionary<string, string>> CookieDic { get; } = new ConcurrentDictionary<string, Dictionary<string, string>>();

        public override Task InitializeAsync(IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }

        public override Task<(IBaseReqParam, Func<IServiceProvider, BaseSpider, IResponseParam, Task>, Func<IgnoreRequestException, Task>)> ProcessRequestAsync(IRequestParam request, BaseSpider spider, ILogger logger)
        {
            if (CookieDic.TryGetValue(request.Uri.Host, out var dic)) {
                foreach (var kv in dic) {
                    request.UserCookie[kv.Key] = kv.Value;
                }
            
            }
            return base.ProcessRequestSetNull();
        }

        public override Task<IRequestParam> ProcessResponseAsync(IRequestParam request, IResponseParam response, BaseSpider spider, ILogger logger)
        {

            if (response.ResponseHeaders.TryGetValue("Set-Cookie", out var tmp_cookie)) {
                var cookie = tmp_cookie as IEnumerable<string>;
                var dic = CookieDic.GetOrAdd(request.Uri.Host, (x) => new Dictionary<string, string>());
                foreach (var i in tmp_cookie)
                {
                    var t = i.Split(";");
                    var j = t[0].Split("=");
                    dic[j[0]] = j.Length > 1 ? j[1] : string.Empty;
                }
            };

            return Task.FromResult((IRequestParam)null);
        }
    }
}
