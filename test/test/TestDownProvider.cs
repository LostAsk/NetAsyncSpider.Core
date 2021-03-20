using NetAsyncSpider.Core;
using NetAsyncSpider.Core.DownProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
namespace test
{
    /// <summary>
    /// 测试默认下载提供者
    /// </summary>
    public class TestDownProvider : BaseDownProvider
    {
        public TestDownProvider(IServiceProvider serviceprovider):base(serviceprovider)
        {
        }


        protected override async Task DownResponseAsync(ResponseParam responseParam)
        {
            var response = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
            foreach (var kv in response.Headers)
            {
                responseParam.ResponseHeaders.Add(kv.Key, kv.Value);
            }
            var bytes = Encoding.UTF8.GetBytes("测试888");
            responseParam.ResponseContent = bytes;
            responseParam.TargetUrl = responseParam.RequestParam.Uri.AbsolutePath;
            responseParam.IsError = response.StatusCode != System.Net.HttpStatusCode.OK;
        }
    }
}
