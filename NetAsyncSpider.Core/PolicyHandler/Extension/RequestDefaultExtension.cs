using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
namespace NetAsyncSpider.Core.PolicyHandler
{
    public static class RequestDefaultExtension
    {
        public static async Task RequestsFuncPolicy(this IServiceProvider serviceProvider, IRequestParam requestParam,BaseSpider baseSpider, Func<PolicyBuilder<ResponseParam>, IServiceProvider,SpiderOptions, RequestParam, Context, IAsyncPolicy<ResponseParam>> Func,Func<IServiceProvider,BaseSpider,IResponseParam,Task> parseasync=null) {
            var hander = serviceProvider.GetService<AnonymousResqustPolicyHander>();
            hander.ConfigurePolicy(Func);
            var single_handler = serviceProvider.GetService<ISingleHandler>();
            await single_handler.ExcuteAsync(requestParam, baseSpider, parseasync);
        }

    }

}
