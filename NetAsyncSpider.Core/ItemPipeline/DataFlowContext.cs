using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System;
using System.Threading.Tasks;
using NetAsyncSpider.Core;
using NetAsyncSpider.Core.ItemPipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NetAsyncSpider.Core.Untils;

namespace NetAsyncSpider.Core.ItemPipeline
{
	/// <summary>
	/// 数据流处理器上下文
	/// </summary>
	public class DataFlowContext : IDisposable
	{
		/// <summary>
		/// 下载器返回的结果
		/// </summary>
		public IResponseParam Response { get; }

		/// <summary>
		/// 消息队列回传的内容
		/// </summary>
		public byte[] MessageBytes { get; internal set; }

		/// <summary>
		/// 下载的请求
		/// </summary>
		public IRequestParam Request => Response?.RequestParam;

		
		/// <summary>
		/// 解析到的目标链接
		/// </summary>
		internal List<IRequestParam> FollowRequests { get; }

		public IServiceProvider ServiceProvider { get; }

		private ILogger<BaseSpider> Logger { get; }
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response">下载器返回的结果</param>
		/// <param name="options"></param>
		/// <param name="serviceProvider"></param>
		public DataFlowContext(IServiceProvider serviceProvider, IResponseParam response)
		{
			Response = response;
			ServiceProvider = serviceProvider;
			FollowRequests = new List<IRequestParam>();
            Logger = serviceProvider.GetService<ILogger<BaseSpider>>();
		}

		public void AddFollowRequests(params IRequestParam[] requests)
		{
			AddFollowRequests(requests.AsEnumerable());
		}

		public void AddFollowRequests(IEnumerable<IRequestParam> requests)
		{

			if (requests != null)
			{
				var deth = Request?.Depth + 1;
				foreach (var request in requests) {
					request.Properties[RequestConstProperties.Depth] = deth;
					request.Properties[RequestConstProperties.RequestedTimes] = 0;
					//request.Properties[RequestConstProperties.BeforeHash] = request.Properties[RequestConstProperties.Hash];
					request.Properties[RequestConstProperties.Hash] = null;
				}
				FollowRequests.AddRange(requests);
			}
		}

		public void AddFollowRequests(IEnumerable<Uri> uris)
		{
			if (uris == null)
			{
				return;
			}
			AddFollowRequests(uris.Select(CreateHttpDefulatNewRequest));
		}

		public IRequestParam CreateHttpDefulatNewRequest(Uri uri)
		{
			uri.NotNull(nameof(uri));
			var request =(IRequestParam) Request.Clone();

			request.Uri = uri;
			return request;
		}

		public void Dispose()
		{
			MessageBytes = null;
			Request?.Dispose();
			Response?.Dispose();

		}
	}
}
