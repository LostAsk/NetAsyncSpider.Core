using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Polly;

namespace NetAsyncSpider.Core
{
    /// <summary>
    /// 策略配置(配合IPolicyHandler使用)
    /// </summary>
    public class CrawlerPolicyBuilderOption
    {
        private Dictionary<string, PolicyBuilder> PolicyBuilders { get; } = new Dictionary<string, PolicyBuilder>();

        private ConcurrentDictionary<Type, Dictionary<string, object>> TPolicyBuilders { get; } = new ConcurrentDictionary<Type, Dictionary<string, object>>();
        /// <summary>
        /// 设置异常策略
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <param name="policyBuilder"></param>
        public void SetPolicyBuilder<TResult>(string key, PolicyBuilder<TResult> policyBuilder)
        {
            var dic= TPolicyBuilders.GetOrAdd(typeof(TResult), (x) => new Dictionary<string, object> ());
            dic[key] = policyBuilder;
        }
        /// <summary>
        /// 设置异常策略
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <param name="policyBuilder"></param>
        public void SetPolicyBuilder(string key, PolicyBuilder policyBuilder)
        {
            PolicyBuilders[key] = policyBuilder;
        }
        /// <summary>
        /// 获取策略
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public PolicyBuilder<TResult> GetPolicyBuilder<TResult>(string key)
        {
            object policy_builder = null;
            TPolicyBuilders[typeof(TResult)].TryGetValue(key, out policy_builder);
            if (policy_builder == null) { throw new Exception($"找不到该类型 {typeof(TResult).Name} 的{key} 策略配置"); };
            return policy_builder as PolicyBuilder<TResult>;
        }
        /// <summary>
        /// 获取策略
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public PolicyBuilder GetPolicyBuilder(string key)
        {
            PolicyBuilder s = null;
            PolicyBuilders.TryGetValue(key, out s);
            if (s == null) { throw new Exception($"找不到该{key} 的策略配置"); };
            return s;
        }
    }
}
