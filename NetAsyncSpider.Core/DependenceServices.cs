using System;
using NetAsyncSpider.Core.MessageQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetAsyncSpider.Core.Scheduler;
using Microsoft.Extensions.Options;
using NetAsyncSpider.Core;
using NetAsyncSpider.Core.Untils;

namespace NetAsyncSpider.Core
{
	public class DependenceServices : IDisposable
	{
		public IHostApplicationLifetime ApplicationLifetime { get; }
		public HostBuilderContext HostBuilderContext { get; }
		public IConfiguration Configuration { get; }

		public DependenceServices(
			IHostApplicationLifetime applicationLifetime,
			IConfiguration configuration,
			HostBuilderContext builderContext,
			IOptions<SpiderOptions> options
			)
		{
			ApplicationLifetime = applicationLifetime;
			HostBuilderContext = builderContext;
			Configuration = configuration;
			/// ×¢²á¶¯Ì¬Ñ¡Ïî
            dynamic obj = Configuration.GetExpandoObjectByAppsetting("SpiderOptions:OtherInfo");
            if (obj != null)
            {
				options.Value.Object = obj.SpiderOptions.OtherInfo;
            }
        }

		public void Dispose()
		{
			
		}
	}
}
