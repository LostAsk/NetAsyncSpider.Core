using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.DownProvider
{
    public interface IDownProvider:IDisposable
    {
        Task<IResponseParam> GetResponseParamAsync(IRequestParam requestParam);
    }


}
