using NetAsyncSpider.Core.DownProvider;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

using System.Net.Http.Headers;
using NetAsyncSpider.Core.Untils;
using NetAsyncSpider.Core.PolicyHandler;
using NetAsyncSpider.Core.Serialize;

namespace NetAsyncSpider.Core
{
    public static class RequestConstProperties {

        public const string Hash = "Hash";

        public const string Depth = "Depth";

        public const string Timestamp = "Timestamp";

        public const string RequestedTimes = "RequestedTimes";

        public const string Priority = "Priority";

        /// <summary>
        /// 代理
        /// </summary>
        public const string Proxy = "Proxy";

    }

    [Serializable]
    public class RequestParam : IRequestParam
    {
        
        public Dictionary<string, dynamic> Properties { get; internal set; } = new Dictionary<string, dynamic>();

        public Uri Uri { get; set; }
        public string Method { get; set; } = HeaderNames.Get;

        public string PolicyBuilderKey { get; set; } = PolicyNames.Default;


        public string Hash=> Properties[RequestConstProperties.Hash];

        public string Owner { get;internal set ; }

        public int Timeout { get; set; } = 0;
        public Type DownProvider { get; internal set; } = typeof(HttpClientDownProvider);

        public Type ResquestPolicyHanderProvider { get; internal set; } = typeof(DefaultRequestPolicyHander);

        private bool _disposed;

        public Version Version { get; set; } = HttpVersion.Version11;

        public string ClientKey { get; set; }
        public int Depth => Properties[RequestConstProperties.Depth];
        public long Timestamp=> Properties[RequestConstProperties.Timestamp];
        public int RequestedTimes => Properties[RequestConstProperties.RequestedTimes];


        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, object> PostData { get; set; } = new Dictionary<string, object>();
        public string Json { get; set; }
        public List<UploadFile> Files { get; set; }
        public Dictionary<string, string> UserCookie { get; set; } = new Dictionary<string, string>();
        public MediaTypeHeaderValue MediaTypeHeaderValue { get; set; }
        public byte[] SendData { get; set; }
        
        public string Encoding { get; set; } = "utf-8";

        

        public RequestParam(string url):this() {
            Uri =new Uri(url);
            RequestSet(this);
        }

        public RequestParam() {

            Properties[RequestConstProperties.Timestamp] = 0;
            Properties[RequestConstProperties.Hash] = string.Empty;
            Properties[RequestConstProperties.Depth] = 0;
            Properties[RequestConstProperties.RequestedTimes] = 0;
            Properties[RequestConstProperties.Priority] = 0;
            requesttype.GetProperty(nameof(Owner)).SetValue(this, cache_dic[nameof(Owner)]);
        }

        public RequestParam CloneSetUri(string url) {
            var p =(RequestParam) Clone();
            p.Uri = new Uri(url);
            return p;
        }


        public object Clone()
        {
            
            var r = new RequestParam
            {
                Uri = Uri,
                Encoding = Encoding,
                Json = Json,
                Owner = Owner,
                Timeout = Timeout,
                Method = Method,
                SendData = SendData,
                PolicyBuilderKey = PolicyBuilderKey,
                Version = Version,
                MediaTypeHeaderValue=MediaTypeHeaderValue,
                DownProvider=DownProvider,
                ResquestPolicyHanderProvider=ResquestPolicyHanderProvider

            };
            if (Headers != null) {
                foreach (var kv in Headers)
                {
                    r.Headers[kv.Key] = kv.Value;
                }
            }
            if (UserCookie != null) {
                foreach (var kv in UserCookie)
                {
                    r.UserCookie[kv.Key] = kv.Value;
                }
            }
            if (PostData != null) {
                foreach (var kv in PostData)
                {
                    r.PostData[kv.Key] = kv.Value;
                }
            }
            
            foreach (var kv in Properties)
            {
                r.Properties[kv.Key] = kv.Value;
            }
           
            return r;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _disposed = true;

            ObjectUtilities.DisposeByDic(Properties);

            //(Content as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Method: ");
            sb.Append(Method);
            sb.Append(", RequestUri: '");
            sb.Append(Uri == null ? "<null>" : Uri.ToString());
            sb.Append("', Version: ");
            sb.Append(Version);
            sb.Append(", Content: ");
            HeaderDumpUtilities.DumpHeaders(sb, PostData);
            sb.AppendLine(", Headers:");
            HeaderDumpUtilities.DumpHeaders(sb, Headers);
            return sb.ToString();
        }

        public void SetDownProvider<TDownProvider>() where TDownProvider : IDownProvider
        {
            DownProvider = typeof(TDownProvider);
        }

        public void SetResquestPolicyHanderProvider<TResqustPolicyHander>() where TResqustPolicyHander : IResqustPolicyHander
        {
            ResquestPolicyHanderProvider = typeof(TResqustPolicyHander);
        }
        private static Dictionary<string, object> cache_dic = new Dictionary<string, object>();
        /// <summary>
        /// 获取属性名
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            //var body = (MemberExpression)expression.Body;
            //return body.Member.Name;
            //MemberInfo member = ((expression.Body as MemberExpression) ?? throw new ArgumentException("MemberExpression is expected in expression.Body", "expression")).Member;
            MemberExpression member = GetMemberExpression<T, TProperty>(expression);
            if (member == null) throw new ArgumentException("MemberExpression is expected in expression.Body", "expression");
            return member.Member.Name;
        }

        private static MemberExpression GetMemberExpression<T, TProperty>(Expression<Func<T, TProperty>> expr)
        {
            var member = expr.Body as MemberExpression;
            var unary = expr.Body as UnaryExpression;
            return member ?? (unary != null ? unary.Operand as MemberExpression : null);
        }
        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        public static void SetDefault<TProperty, TValue>(Expression<Func<RequestParam, TProperty>> expression, TValue value)
        {
            var name = GetPropertyName<RequestParam, TProperty>(expression);
            if (name == "Properties"||name== "Owner") return;
            cache_dic[name] = value;
        }

        internal static void InternalSetDefault<TProperty, TValue>(Expression<Func<RequestParam, TProperty>> expression, TValue value)
        {
            var name = GetPropertyName<RequestParam, TProperty>(expression);
            cache_dic[name] = value;
        }

        private static Type requesttype = typeof(RequestParam);
        private static void RequestSet(RequestParam request)
        {
            foreach (var kv in cache_dic)
            {
                requesttype.GetProperty(kv.Key).SetValue(request, kv.Value);
            }
        }
    }

    public class ResponseParam : IResponseParam
    {
        public ResponseHeader ResponseHeaders { get; } = new ResponseHeader();

        public bool IsExceptionCause => ErrorMessage!=null;
        public Exception ErrorMessage { get; set; }

        public bool IsError { get;  set; }

        public byte[] ResponseContent { get; set; }

        public int ElapsedMilliseconds { get; set; }

        public string TargetUrl { get; set; }

        public IRequestParam RequestParam { get; set; }

        public int? Status { get; set; }

        public Dictionary<string, dynamic> Properties => RequestParam.Properties;

      

        private bool _disposed = false;


        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _disposed = true;

            ObjectUtilities.DisposeByDic(ResponseHeaders);
            ResponseContent = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
