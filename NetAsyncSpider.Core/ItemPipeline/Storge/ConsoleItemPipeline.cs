using System;
using System.Threading.Tasks;
using NetAsyncSpider.Core;
using NetAsyncSpider.Core.ItemPipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using NetAsyncSpider.Core.Scheduler;
using System.Text;

namespace NetAsyncSpider.Core.ItemPipeline
{
	/// <summary>
	/// 控制台打印解析结果(所有解析结果)
	/// </summary>
	public class ConsoleItemPipeline : BaseItemPipeline
	{
   

        public static IItemPipeline CreateFromOptions(IConfiguration configuration)
		{
			return new ConsoleItemPipeline();
		}

        public override Task HandleAsync(BaseSpider baseSpider, IResponseParam responseParam, ILogger logger, IScheduler scheduler)
        {
			if (IsNullOrEmpty(responseParam))
			{
				logger.LogWarning("数据流上下文不包含解析结果");
				return Task.CompletedTask;
			}

			var data = responseParam.ResponseHeaders.ToImmutableDictionary();
			var k=data.Remove(nameof(Encoding));
			Console.ForegroundColor = ConsoleColor.Cyan;
			logger.LogInformation(
				$"{Environment.NewLine}DATA: {System.Text.Json.JsonSerializer.Serialize(k)}");

			return Task.CompletedTask;
		}

        public override Task InitializeAsync(IServiceProvider serviceProvider)
        {
			return Task.CompletedTask;
        }
    }
}
