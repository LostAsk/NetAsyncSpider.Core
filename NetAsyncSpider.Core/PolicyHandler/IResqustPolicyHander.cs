using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.PolicyHandler
{
    /// <summary>
    /// 请求策略操作 建议瞬时
    /// </summary>
    public interface IResqustPolicyHander :IPolicyHander<RequestParam,ResponseParam>
    {
    }
}
