using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetAsyncSpider.Core.HashService;
using NetAsyncSpider.Core.Scheduler.Component;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace NetAsyncSpider.Core.Scheduler
{
	/// <summary>
	/// 基于内存的广度优先调度(去重 URL)
	/// </summary>
	public class BfsScheduler : SchedulerBase
	{
		private readonly List<IRequestParam> _requests =
			new List<IRequestParam>();

		public BfsScheduler(IDuplicateRemover duplicateRemover, IHashAlgorithmService requestHasherm, IOptions<SpiderOptions> options, ILogger<BaseSpider> logger) :
			base(duplicateRemover, requestHasherm, options, logger)
		{
			//comparer = new AnonymousComparer<IRequestParam>((x, y) => { return y.Depth.CompareTo(x.Depth); });
		}

		public override void Dispose()
		{
			_requests.Clear();
			base.Dispose();
		}

		/// <summary>
		/// 如果请求未重复就添加到队列中
		/// </summary>
		/// <param name="request">请求</param>
		protected override Task PushWhenNoDuplicate(IRequestParam request,IResponseParam responseParam)
		{
			if (SpiderOption.Depth>0&&request.Depth > SpiderOption.Depth)
			{
				Logger.LogDebug($"当前深度:{request.Depth} > {SpiderOption.Depth} 链接为:{request.Uri}");
			}
			else
			{
				_requests.Add(request);
			}
			return Task.CompletedTask;
		}

		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		protected override Task<IRequestParam[]> ImplDequeueAsync(int count = 1)
		{
			var requests = _requests.Take(count).ToArray();
			//var requests_count = requests.Count();
			if (requests.Length > 0)
			{
				_requests.RemoveRange(0, requests.Length);
			}
			return Task.FromResult( requests);

		}
		public override long GetTotal()
		{
			return _requests.Count;
		}
	}
}
