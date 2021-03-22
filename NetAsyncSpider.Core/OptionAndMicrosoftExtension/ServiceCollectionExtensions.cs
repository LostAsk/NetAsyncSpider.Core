using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.DownProvider;
using NetAsyncSpider.Core.HashService;
using NetAsyncSpider.Core.MessageQueue;
using NetAsyncSpider.Core.Scheduler.Component;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NetAsyncSpider.Core.Scheduler;
using NetAsyncSpider.Core.PolicyHandler;
using Polly;
using NetAsyncSpider.Core.Untils;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using NetAsyncSpider.Core.RequestPipeline;

namespace NetAsyncSpider.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 使用MD5进行 hash requests
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseMD5HashAlgorithmService(this IServiceCollection services) {
            services.Replace(ServiceDescriptor.Singleton(typeof(IHashAlgorithmService), typeof(MD5HashAlgorithmService)));
            return services;
        }
        /// <summary>
        /// 注册默认服务(IDataFlow,IDownProvider)需要另行注册
        /// </summary>
        /// <typeparam name="TScheduler"></typeparam>
        /// <typeparam name="TSpider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultSpiderService<TScheduler,TSpider>(this IServiceCollection services)
            where TScheduler : class, IScheduler
            where TSpider : BaseSpider
        {
            return AddSpiderService<TScheduler, CommunicationMessage, MD5HashAlgorithmService, HashSetDuplicateRemover, DefaultRequestPolicyHander, TSpider>(services);
        }

        /// <summary>
        /// 注册请求中间件
        /// </summary>
        /// <typeparam name="TScheduler"></typeparam>
        /// <typeparam name="TSpider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRequestMiddlewareService(this IServiceCollection services,params Type[] requestMiddlewares)
        {
            foreach (var i in requestMiddlewares) {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware), i));
            }
            return services;
        }

        /// <summary>
        /// 注册管道中间件
        /// </summary>
        /// <typeparam name="TScheduler"></typeparam>
        /// <typeparam name="TSpider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddItemPipelineService(this IServiceCollection services, params Type[] itemPipelineMiddlewares)
        {
            foreach (var i in itemPipelineMiddlewares)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IItemPipeline), i));
            }
            return services;
        }
        /// <summary>
        /// 注册服务(IDataFlow,IDownProvider)需要另行注册
        /// </summary>
        /// <typeparam name="TScheduler"></typeparam>
        /// <typeparam name="TCommunicationMessage"></typeparam>
        /// <typeparam name="THashAlgorithmService"></typeparam>
        /// <typeparam name="TDuplicateRemover"></typeparam>
        /// <typeparam name="TResqustPolicyHander"></typeparam>
        /// <typeparam name="TSpider"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSpiderService<TScheduler, TCommunicationMessage, THashAlgorithmService, TDuplicateRemover, TResqustPolicyHander,TSpider>(this IServiceCollection services)
            where TScheduler:class, IScheduler
            where TCommunicationMessage : class, ICommunicationMessage
            where THashAlgorithmService : class, IHashAlgorithmService
            where TDuplicateRemover : class, IDuplicateRemover
            where TResqustPolicyHander : class, IResqustPolicyHander
            where TSpider:BaseSpider
        {

            services.AddHttpClient();
           
            //services.Replace(ServiceDescriptor.Transient(typeof(HttpMessageHandlerBuilder),typeof(NetCoreDefaultHttpMessageHandlerBuilder)));
            services.AddTransient<HttpClientDownProvider>();
            services.AddTransient(typeof(Pipeline<>));
            services.TryAddSingleton<DependenceServices>();
            services.TryAddSingleton<IScheduler, TScheduler>();
            services.TryAddSingleton<ICommunicationMessage, TCommunicationMessage>();
            services.TryAddSingleton<ISingleHandler, SingleHandler>();
            services.TryAddSingleton<IHashAlgorithmService, THashAlgorithmService>();
            services.TryAddSingleton<IDuplicateRemover, TDuplicateRemover>();
            services.TryAddTransient<IResqustPolicyHander, TResqustPolicyHander>();
            services.AddTransient<DefaultRequestPolicyHander>();
            var servicedescript = ServiceDescriptor.Singleton(typeof(IItemPipeline), typeof(ConsoleItemPipeline));
            services.TryAddEnumerable(servicedescript);
            services.Configure<CrawlerPolicyBuilderOption>((z) =>
            {
                var default_policy = Policy.Handle<Exception>().OrResult<ResponseParam>(x => x.IsError);
                z.SetPolicyBuilder(PolicyNames.Default, default_policy);
            });
            services.AddSingleton<AsyncSpiderCoreEngine>();
            services.AddHostedService<TSpider>();
            var config = services.GetConfiguration();
            var s = config.GetSection("SpiderOptions");
            services.Configure<SpiderOptions>(s);

            //dynamic obj = config.GetExpandoObjectByAppsetting("Options:OtherInfo");
            //if (obj != null)
            //{
            //    services.Configure<SimpleOptions>(x =>
            //    {

            //        x.Object = obj.SpiderOption.OtherInfo;
            //    });
            //}

            return services;
        }



        /// <summary>
        /// 替换configuretion配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ReplaceConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            return services.Replace(ServiceDescriptor.Singleton<IConfiguration>(configuration));
        }

        public static IConfiguration GetConfiguration(this IServiceCollection services)
        {
            var hostBuilderContext = services.GetSingletonInstanceOrNull<HostBuilderContext>();
            if (hostBuilderContext?.Configuration != null)
            {
                return hostBuilderContext.Configuration as IConfigurationRoot;
            }

            return services.GetSingletonInstance<IConfiguration>();
        }
    }
}
