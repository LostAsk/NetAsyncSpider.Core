using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core
{
    public interface IResponseParam: IBaseReqParam,IDisposable
    {
        ResponseHeader ResponseHeaders { get; }
        /// <summary>
        /// Policy的Exception不为空 就是策略 OnResult为true时 Policy的Exception为空的
        /// </summary>
        bool IsExceptionCause { get; }
        /// <summary>
        /// 错误信息
        /// </summary>
        Exception ErrorMessage { get;}
        /// <summary>
        /// 是否出错
        /// </summary>
        bool IsError { get; }
        /// <summary>
        /// 状态码
        /// </summary>
        int? Status { get; set; }
        /// <summary>
        /// 响应bytes
        /// </summary>
        byte[] ResponseContent { get; set; }
        /// <summary>
		/// 下载消耗的时间
		/// </summary>
	    int ElapsedMilliseconds { get; set; }

        public string TargetUrl { get; }

        IRequestParam RequestParam { get; set; }
        /// <summary>
        /// IRequestParam.Properties
        /// </summary>
        Dictionary<string, dynamic> Properties { get; }

       
    }
}
