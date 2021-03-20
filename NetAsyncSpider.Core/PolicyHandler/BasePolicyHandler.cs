using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace NetAsyncSpider.Core.PolicyHandler
{
    /// <summary>
    /// 生成异常操作基类<br></br>
    /// 主要是根据Key找到相应的定义异常builder
    /// </summary>
    public abstract class BasePolicyHandler
    {

        protected IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        /// 策略上下文
        /// 全局的！！！！
        /// </summary>
        protected Context PolicyContext { get; } = new Context();
        /// <summary>
        /// 策略异常Builder选项
        /// </summary>
        private CrawlerPolicyBuilderOption Options { get; }
        protected BasePolicyHandler(IServiceProvider serviceProvider) {
            ServiceProvider = serviceProvider;
            Options = ServiceProvider.GetService<IOptions<CrawlerPolicyBuilderOption>>().Value;
        }

        protected PolicyBuilder<TResult> GetPolicyBuilder<TResult>(string key)
        {
            return Options.GetPolicyBuilder<TResult>(key);
        }

        protected PolicyBuilder GetPolicyBuilderNonGeneric(string key)
        {
            return Options.GetPolicyBuilder(key);
        }

    }
    /// <summary>
    /// 定义策略基类
    /// </summary>
    /// <typeparam name="TInput">更新外部参数</typeparam>
    /// <typeparam name="TResult"></typeparam>
    public abstract class BasePolicyHandler<TInput,TResult>: BasePolicyHandler, IPolicyHander<TInput,TResult> 
    {
        protected TInput Param => (TInput)PolicyContext[InputKey] ;

        protected virtual string InputKey { get; private set; }

        protected BasePolicyHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
        /// <summary>
        /// 定义策略
        /// </summary>
        /// <param name="policyBuilder"></param>
        /// <returns></returns>
        protected abstract IAsyncPolicy<TResult> SetAsyncPolicy(PolicyBuilder<TResult> policyBuilder);
        /// <summary>
        /// 策略运行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract Task<TResult> ExcuteTaskAsync(Context context);

        protected abstract Task<TResult> ExecuteAndCaptureAfterHander(PolicyResult<TResult> policyResult, Context context, ILogger logger);
        public virtual async Task<TResult> ExecuteAndCaptureAsync(string key,ILogger logger)
        {
            var policyBuilder = GetPolicyBuilder<TResult>(key);
            var poliy = SetAsyncPolicy(policyBuilder);
            var po_reulst= await poliy.ExecuteAndCaptureAsync(ExcuteTaskAsync, base.PolicyContext);
            var result=await ExecuteAndCaptureAfterHander(po_reulst, base.PolicyContext, logger);

            return result;
        }
        /// <summary>
        /// 相当于Set ParamName
        /// </summary>
        /// <param name="input"></param>
        public void SetInput(string key,TInput input)
        {
            InputKey = key;
            base.PolicyContext.TryAdd(InputKey, input);
        }

        public virtual void Dispose()
        {
          
        }

        public void CrearInputByKey(string key)
        {
            base.PolicyContext.Remove(key);
        }
    }
}
