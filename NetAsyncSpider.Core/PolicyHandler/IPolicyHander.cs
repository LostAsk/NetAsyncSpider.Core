using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace NetAsyncSpider.Core.PolicyHandler
{
    public interface IPolicyHander<TInput> : IDisposable {
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="input"></param>
        void SetInput(string key,TInput input);
        /// <summary>
        /// 清除参数
        /// </summary>
        /// <param name="key"></param>
        void CrearInputByKey(string key);
    }


    public interface IPolicyHander<TInput,TResult> : IPolicyHander<TInput>
    {

        /// <summary>
        /// 运行策略方案
        /// </summary>
        /// <param name="policy_bulider_key">策略构建标识</param>
        /// <returns></returns>
        Task<TResult> ExecuteAndCaptureAsync(string policy_bulider_key,ILogger logger);
    }
}
