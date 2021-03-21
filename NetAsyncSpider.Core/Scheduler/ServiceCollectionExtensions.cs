using NetAsyncSpider.Core.Scheduler.Component;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NetAsyncSpider.Core.Scheduler;

namespace NetAsyncSpider.Core.Scheduler
{
	public static class ServiceCollectionExtensions
	{
		public static IHostBuilder UseQueueDistinctBfsScheduler<TDuplicateRemover>(this IHostBuilder builder)
			where TDuplicateRemover : class, IDuplicateRemover
		{
			builder.ConfigureServices(x =>
			{
				x.TryAddSingleton<IDuplicateRemover, TDuplicateRemover>();
				x.TryAddSingleton<IScheduler, BfsScheduler>();
			});
			return builder;
		}
	}
}
