using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System;
namespace NetAsyncSpider.Core.DownProvider
{
	/// <summary>
	/// HttoClientHandler代理<bra></bra>
	/// 不要在并发过程
	/// </summary>
    public class HttpClientProxyHandler: HttpClientHandler
    {
		private Action<HttpClientProxyHandler, HttpRequestMessage> BeforeSendSet;
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			
			var response = await base.SendAsync(request, cancellationToken);
			return response;

		}

	}
}
