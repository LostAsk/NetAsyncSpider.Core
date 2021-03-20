using Microsoft.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.DownProvider
{
    /// <summary>
    /// 请求基类(建议瞬时)
    /// </summary>
    public abstract class BaseDownProvider : IDownProvider
    {
        static BaseDownProvider() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected IServiceProvider ServiceProvider { get; }

        protected ResponseParam ResponseParam { get; private set; } = new ResponseParam();
        protected BaseDownProvider(IServiceProvider serviceProvider) {
            ServiceProvider = serviceProvider;
        }

        public virtual void Dispose()
        {
           
        }

        protected abstract Task DownResponseAsync(ResponseParam responseParam);
        public async Task<IResponseParam> GetResponseParamAsync(IRequestParam requestParam)
        {
            
            ResponseParam.RequestParam = requestParam;
            await DownResponseAsync(ResponseParam);
            return ResponseParam;
        }
    }
}
