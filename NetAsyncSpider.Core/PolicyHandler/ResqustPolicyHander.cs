using NetAsyncSpider.Core.MessageQueue;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetAsyncSpider.Core.DownProvider;

namespace NetAsyncSpider.Core.PolicyHandler
{
    /// <summary>
    /// 请求策略操作类
    /// </summary>
    public abstract class ResqustPolicyHander : BasePolicyHandler<RequestParam,ResponseParam>, IResqustPolicyHander
    {
        protected ICommunicationMessage CommunicationMessage { get; }
        public ResqustPolicyHander(IServiceProvider serviceProvider):base(serviceProvider) {
            CommunicationMessage=serviceProvider.GetService<ICommunicationMessage>();
        }
        /// <summary>
        /// 内部就是生成IDownProvider生成ResponseParam
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override async Task<ResponseParam> ExcuteTaskAsync(Context context)
        {
            using (var DownProvider = (IDownProvider)ServiceProvider.GetService(base.Param.DownProvider) )
            return (ResponseParam)await DownProvider.GetResponseParamAsync(base.Param);
        }

    }
}
