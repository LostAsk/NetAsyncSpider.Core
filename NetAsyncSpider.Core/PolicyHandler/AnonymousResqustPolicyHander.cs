using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.PolicyHandler
{
    /// <summary>
    /// 匿名策略生成ResqustPolicyHander
    /// </summary>
    public class AnonymousResqustPolicyHander : DefaultRequestPolicyHander
    {
        private Func<PolicyBuilder<ResponseParam>, IServiceProvider, SpiderOptions, RequestParam, Context, IAsyncPolicy<ResponseParam>> Func { get; set; }
        public AnonymousResqustPolicyHander(IServiceProvider serviceProvider, IOptions<SpiderOptions> options) : base(serviceProvider, options) { 
        
        }
        protected override IAsyncPolicy<ResponseParam> SetAsyncPolicy(PolicyBuilder<ResponseParam> policyBuilder)
        {
            return Func(policyBuilder, ServiceProvider,base.SpiderOptions, base.Param, base.PolicyContext);
        }

        public AnonymousResqustPolicyHander ConfigurePolicy(Func<PolicyBuilder<ResponseParam>, IServiceProvider, SpiderOptions, RequestParam, Context, IAsyncPolicy<ResponseParam>> func) {

            Func = func;
            return this;
        }

    }
}
