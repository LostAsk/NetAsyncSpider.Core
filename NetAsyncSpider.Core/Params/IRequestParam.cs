
using NetAsyncSpider.Core.DownProvider;
using NetAsyncSpider.Core.PolicyHandler;
using NetAsyncSpider.Core.Untils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core
{
    public interface IRequestParam: IBaseReqParam,IDisposable, ICloneable
    {
		Dictionary<string,string> Headers { get; set; }

		/// <summary>
		/// 额外参数
		/// </summary>
		Dictionary<string, dynamic> Properties { get; }

		/// <summary>
		/// Post参数
		/// </summary>
		public Dictionary<string, object> PostData { get; set; }

        /// <summary>
        /// post传Json参数
        /// </summary>
        public string Json { get; set; }
		/// <summary>
		/// httpclientfactory 创建httpclient 标识，可为空
		/// </summary>
		public string ClientKey { get; set; }
		/// <summary>
		/// 上传文件参数
		/// </summary>
		public List<UploadFile> Files { get; set; }
		/// <summary>
		/// 自定义cookie
		/// </summary>
		public Dictionary<String, String> UserCookie { get; set; }

        /// <summary>
        /// 自定义设置头MediaType （配合UserSendData一起用）
        /// </summary>
        public MediaTypeHeaderValue MediaTypeHeaderValue { get; set; }

        /// <summary>
        /// 发送参数
        /// </summary>
        public byte[] SendData { get; set; }
		/// <summary>
		/// 编码 <br></br>
		/// https://blog.csdn.net/gengyiping18/article/details/77620061
		/// </summary>
		public string Encoding { get; set; }
		/// <summary>
		/// 必填
		/// </summary>
		Uri Uri { get; set; }
		/// <summary>
		/// 必填
		/// </summary>
	    string Method { get; set;}

		/// <summary>
		/// 策略名(必填)
		/// </summary>
		string PolicyBuilderKey { get; set; }

		///// <summary>
		///// 数据处理提供者名称	
		///// </summary>
		//List<string> DataFlowProviderName { get; set; }
		/// <summary>
		/// 请求的哈希
		/// </summary>
		string Hash { get; }

		/// <summary>
		/// 任务标识(必填)
		/// </summary>
		string Owner { get;}

		/// <summary>
		/// Http版本
		/// </summary>
		Version Version { get;set; }
		/// <summary>
		/// 请求的 Timeout 时间(最小2秒)
		/// </summary>
		int Timeout { get;set; }


		/// <summary>
		/// 链接的深度
		/// </summary>
		int Depth { get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        long Timestamp { get; }
        /// <summary>
        /// 已经重试的次数
        /// </summary>
        int RequestedTimes { get; }
		/// <summary>
		/// 下载提供者
		/// </summary>
		Type DownProvider { get; }

		Type ResquestPolicyHanderProvider { get; }
		void SetDownProvider<TDownProvider>() where TDownProvider:IDownProvider;
		void SetResquestPolicyHanderProvider<TResqustPolicyHander>() where TResqustPolicyHander : IResqustPolicyHander;

		RequestParam CloneSetUri(string url);
	}
}
