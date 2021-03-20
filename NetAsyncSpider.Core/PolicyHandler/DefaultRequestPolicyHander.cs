using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using NetAsyncSpider.Core.Scheduler;

using Microsoft.Extensions.Logging;

namespace NetAsyncSpider.Core.PolicyHandler
{
    /// <summary>
    /// 默认请求策略操作
    /// </summary>
    public class DefaultRequestPolicyHander : ResqustPolicyHander
    {
        protected SpiderOptions SpiderOptions { get; }
        protected ILogger<BaseSpider> Logger { get; }
        public DefaultRequestPolicyHander(IServiceProvider serviceProvider,IOptions<SpiderOptions> options) : base(serviceProvider) { 
            SpiderOptions = options.Value;
            Logger = serviceProvider.GetService<ILogger<BaseSpider>>();
        }
        protected override IAsyncPolicy<ResponseParam> SetAsyncPolicy(PolicyBuilder<ResponseParam> policyBuilder)
        {
            var retry= policyBuilder.RetryAsync(SpiderOptions.RetriedTimes, (exception, retryCount, content) =>
            {
                var schdeult = ServiceProvider.GetService<IScheduler>();
                Param.Properties[RequestConstProperties.RequestedTimes] += 1;
                Logger.LogError($"Id:{Param.Owner} 第{ Param.RequestedTimes} 次---- 链接:{Param.Uri} 异常信息:{exception.Exception?.Message}");
                exception.Result?.Dispose();
            });
            if (Param.Timeout <= 0) {
                return retry;
            }
            ///超时设置;Param.Timeout
            var timeoutPolicy = Policy.TimeoutAsync(Param.Timeout, TimeoutStrategy.Pessimistic, (context, timespan, task) =>
            {
                var schdeult = ServiceProvider.GetService<IScheduler>();
                Param.Properties[RequestConstProperties.RequestedTimes] += 1;
                Logger.LogError($"{Param.Owner} 链接:{Param.Uri} 下载超时:{timespan.TotalSeconds}");
                return Task.CompletedTask;
            });
            return retry.WrapAsync(timeoutPolicy);
        }


        protected override Task<ResponseParam> ExecuteAndCaptureAfterHander(PolicyResult<ResponseParam> policyResult, Context context, ILogger logger)
        {
            ResponseParam reponse = null ;
            if (policyResult.Result == null && policyResult.FinalHandledResult == null)
            {
                logger.LogError($"Id:{base.Param.Owner} Url:{base.Param.Uri} 下载失败{ policyResult.FinalException.Message}");
                reponse = new ResponseParam { ErrorMessage = policyResult.FinalException, IsError = true, RequestParam = base.Param, TargetUrl = Param.Uri.ToString() };
            }
            else {
                reponse = policyResult.Outcome == Polly.OutcomeType.Successful ? policyResult.Result : policyResult.FinalHandledResult;
                reponse.IsError = policyResult?.FaultType != null;
                reponse.ErrorMessage = policyResult?.FinalException;
                Logger.LogDebug($"Id:{Param.Owner} Url:{Param.Uri} 下载完成");
            }
            return Task.FromResult(reponse);
        }
    }
}
