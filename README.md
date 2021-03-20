<h1 align="center"> NetAsyncSpider.Core </h1>
<div align="center">
免责申明：本框架是为了帮助开发人员简化开发流程、提高开发效率，请勿使用此框架做任何违法国家法律的事情，使用者所做任何事情也与本框架的作者无关。

NetAsyncSpider.Core 是一款轻量、高配置的、易扩展、强大的抓取异步框架,支持 .NET Core 3+、.NET5+

</div>
- 支持 自定义的请求策略；
- 支持 自定义的下载器；
- 支持 添加自定义的多个请求中间件；
- 支持 添加自定义的多个解析中间件；
- 支持 分布式爬虫 （暂没实现,开发者可以实现ICommunicationMessage替换组件即可）；

QQ群：867954883(在线)
> dotnet add package NetAsyncSpider.Core
## 🚀 快速入门
```csharp
    /// <summary>
    /// 测试spider
    /// </summary>
    public class TestSprider : BaseSpider
    {
        public TestSprider(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }
        /// <summary>
		/// 主要作用是产生新的requestparam<br></br>
		/// 解析可由中间件解析<br></br>
		/// 或者在这里解析,放到参数IResponseParam,中间件获取解析对象进行操作
		/// </summary>
		/// <param name="responseParam"></param>
		/// <returns></returns> 
        public override async Task ParseAsync(IResponseParam responseParam)
        {
            ///测试递归
            Console.WriteLine($"url :{responseParam.RequestParam.Uri} 第{responseParam.RequestParam.Depth}层节点");
            var x = responseParam.TargetUrl.Split("/")[^1..][0];
            var tmp = int.Parse(x) + 2;
            var url = $"https://localhost:5001/weatherforecast/{tmp}";
            var k = responseParam.RequestParam.CloneSetUri(url);
            k.Method = HeaderNames.Head;
            ///推送任务
            await Scheduler.EnqueueAsync(k, responseParam);
        }
        /// <summary>
		/// 请求预处理<br></br>
		/// 可设置每个请求之前的时间间隔防止太快<br></br>
		/// 由于httpclientfactory要预设置<br></br>
		/// 所以通过设置IRequestParam.ClientKey="xxx"找到对应的httpclient
		/// </summary>
		/// <param name="requestParam"></param>
		/// <returns></returns>
        public override Task PreRequest(IRequestParam requestParam)
        {
            return Task.CompletedTask;
        }


        protected override async Task InitializeAsync(CancellationToken stoppingToken = default)
        {
            ///默认超时为0
            RequestParam.SetDefault(x => x.Timeout, 0);
            ///默认使用刚才注册的定义策略
            RequestParam.SetDefault(x => x.PolicyBuilderKey, "test");
            ///默认使用定义的下载器
            RequestParam.SetDefault(x => x.DownProvider,typeof(TestDownProvider));
            var test = Enumerable.Range(0,2).Select(x => new RequestParam($"https://localhost:5001/weatherforecast/{x}") { }).ToList();
            test[0].Properties.Add(RequestConstProperties.Proxy, "fff");//[] = "ffff";
            foreach (var i in test) {
                ///第一次推送任务用这个方法
                await Scheduler.FirstEnqueueAsync(i, null, null);
            }
        }
    }
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


            ///使用TestSprider,
            var pHost = Host.CreateDefaultBuilder().ConfigureDefaultSpiderBuilder<QueueDistinctDfsScheduler, TestSprider>((context, services) => {
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
```
