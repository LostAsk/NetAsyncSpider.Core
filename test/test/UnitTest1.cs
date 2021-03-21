using Microsoft.Extensions.Hosting;
using NetAsyncSpider.Core.Scheduler;
using NetAsyncSpider.Core;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Generic;
using NetAsyncSpider.Core.Untils;
using NetAsyncSpider.Core.Untils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace test
{
    public class Program
    {

        public static async Task Main(string[] args)
        {

            ///定义log的实例配置 这里是用Serilog
            Log.Logger = new LoggerConfiguration()
                
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Error)
                .Enrich.FromLogContext()
                 //.WriteTo.RollingFile("Logs/logs.txt", LogEventLevel.Debug | LogEventLevel.Information)
                .WriteTo.Async(c => c.Console(LogEventLevel.Information))
                .WriteTo.Async(c => c.File("Logs/logs.txt",LogEventLevel.Error))
               
                .CreateLogger();


            ///使用DfsScheduler,TestSprider,
            var pHost = Host.CreateDefaultBuilder().ConfigureDefaultSpiderBuilder<DfsScheduler, TestSprider>((context, services) => {
                ///注册添加刚才定义的下载器
                services.AddTransient<TestDownProvider>();
                ///配置自定义策略
                services.Configure<CrawlerPolicyBuilderOption>(x =>
                {
                    ///定义失败的策略
                    var testpolicy = Policy.Handle<Exception>().OrResult<ResponseParam>(z => z.IsError);
                    x.SetPolicyBuilder("test", testpolicy);
                });
            }, null, args)
            ///使用日志
            .UseSerilog();
            ///运行
            await pHost.RunConsoleAsync();

        }

        ///AddItemPipelineService 注册刚才定义的管道中间件
        services.AddItemPipelineService(typeof(TestItemPipeline));
       

    }
}