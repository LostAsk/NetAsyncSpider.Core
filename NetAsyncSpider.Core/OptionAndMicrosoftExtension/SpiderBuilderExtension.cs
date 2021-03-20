using NetAsyncSpider.Core.ItemPipeline;
using NetAsyncSpider.Core.DownProvider;
using NetAsyncSpider.Core.MessageQueue;
using NetAsyncSpider.Core.HashService;
using NetAsyncSpider.Core.Scheduler;
using NetAsyncSpider.Core.Scheduler.Component;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace NetAsyncSpider.Core
{
	/// <summary>
	/// 爬虫构建器
	/// </summary>
	public static class SpiderBuilderExtension 
	{
		public static IHostBuilder ConfigureDefaultSpiderBuilder<TScheduler,TSpider>(this IHostBuilder builder,
			Action<HostBuilderContext, IServiceCollection> configure_services=null, 
			Action<HostBuilderContext, IConfigurationBuilder> configure_builder=null, 
			string[] args = null)
			where TScheduler : class, IScheduler
			where TSpider : BaseSpider
		{
			Action<HostBuilderContext, IServiceCollection> actions = (context, services) => {

				services.AddDefaultSpiderService<TScheduler, TSpider>();
				configure_services?.Invoke(context, services);
			};
			return ConfigureSpiderBuilder(builder, actions, configure_builder, args);
		}




		public static IHostBuilder ConfigureSpiderBuilder(this IHostBuilder builder, Action<HostBuilderContext,IServiceCollection> configure_services,Action<HostBuilderContext,IConfigurationBuilder> configure_builder, string[] args = null)
		{
			builder.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureHostConfiguration((config) => {
					config.AddEnvironmentVariables("SIMPLIE_SPIDER_");
					if (args == null) return;
					config.AddCommandLine(args);
				})
				.ConfigureAppConfiguration(
				(hostingContext, config) =>
				{
					var env = hostingContext.HostingEnvironment;
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					///添加指定的 环境配置
					.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
					///添加环境变量到配置 IConfiguration
					.AddEnvironmentVariables();
					//添加额外的配置action
					configure_builder?.Invoke(hostingContext, config);
					if (args != null) {
						///添加命令行参数
						config.AddCommandLine(args);
					}
					
				})
				//配置log
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
					logging.AddEventSourceLogger();
				})
				.ConfigureServices((context, services) =>
				{
					configure_services?.Invoke(context, services);
				}).UseDefaultServiceProvider((context, options) =>
				{
					var flag = context.HostingEnvironment.IsDevelopment();
					options.ValidateScopes = flag;
					options.ValidateOnBuild = flag;
				});
			return builder;
		}


    }
}
