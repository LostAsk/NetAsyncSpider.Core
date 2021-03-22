using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using NetAsyncSpider.Core.Untils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.DownProvider
{


    public class HttpClientDownProvider : BaseDownProvider
    {
        private static HashSet<string> _hashBodyMethods = new HashSet<string>() {"GET", "POST","HEAD", "OPTIONS", "PATCH", "PUT", "DELETE", "Trace" };
        protected IHttpClientFactory HttpClientFactory { get; }

        private const string Agent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36";

        protected readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;

        public HttpClientDownProvider(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor) : base(serviceProvider) {
            HttpClientFactory = httpClientFactory;
            _optionsMonitor = optionsMonitor;
        }

        protected virtual HttpClient GetHttpClient(IRequestParam requestParam) {
            var key = string.IsNullOrEmpty( requestParam.ClientKey) ? $"{requestParam.Owner}_{requestParam.Uri.Host}" : requestParam.ClientKey;
            var client= HttpClientFactory.CreateClient(key);
            return client;
        }

        protected override async Task DownResponseAsync(ResponseParam responseParam)
        {
            var requestParam = responseParam.RequestParam;
           
            var http_client = GetHttpClient(requestParam);
     
            using (var httprequestsmessage = GetHttpRequestMessage(requestParam)) {
                await HttpMessageHandler(http_client,httprequestsmessage, requestParam);
                using (var resopne = await http_client.SendAsync(httprequestsmessage))
                {
                    await HttpResposeHandler(resopne, responseParam);
                }
            }  
        }
        /// <summary>
        /// 请求message预处理
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        protected virtual Task HttpMessageHandler(HttpClient httpClient,HttpRequestMessage httpRequestMessage, IRequestParam requestParam) {
            if (httpRequestMessage.Headers.UserAgent.Count == 0) httpRequestMessage.Headers.Add(HeaderNames.UserAgent, Agent);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 响应处理
        /// </summary>
        /// <param name="resopne"></param>
        /// <param name="responseParam"></param>
        /// <returns></returns>
        protected virtual async Task HttpResposeHandler(HttpResponseMessage resopne, ResponseParam responseParam) {
            responseParam.ResponseHeaders.HttpStatusCode= resopne.StatusCode;
            foreach (var kv in resopne.Headers)
            {
                responseParam.ResponseHeaders.Add(kv.Key, kv.Value);
            }
            var bytes = await resopne.Content.ReadAsByteArrayAsync();
            var encoding = GetEncoding(resopne, bytes);
            responseParam.ResponseHeaders.Add(nameof(Encoding), encoding);
            responseParam.TargetUrl = resopne.RequestMessage.RequestUri.AbsoluteUri;
            responseParam.ResponseContent = bytes;
            responseParam.IsError = resopne.StatusCode != System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// 生成HttpRequestMessage
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        protected virtual HttpRequestMessage GetHttpRequestMessage(IRequestParam requestParam)
        {
            var s = _hashBodyMethods.TryGetValue(requestParam.Method.ToUpper(), out var method);

            if (!s) { throw new Exception($"{requestParam.Uri}的请求方式出错啦:Method为{requestParam.Method}"); }
            var encoding = Encoding.GetEncoding(requestParam.Encoding);
           
            var httpmessage = new HttpRequestMessage(new HttpMethod(method), requestParam.Uri);
          
            CreateCookieHeader(httpmessage.Headers, requestParam.UserCookie);
            RequstsHelper.UpdateRequestHeader(httpmessage.Headers, requestParam.Headers);
            httpmessage.Content = CreateContent(requestParam);
            return httpmessage;
        }

        /// <summary>
        /// 创建HttpContent对象
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        protected static HttpContent CreateContent(IRequestParam requestParam)
        {
            HttpContent http = null;
            var method = requestParam.Method.ToUpper();
            var encoding = Encoding.GetEncoding(requestParam.Encoding);
            var get_head = new string[] { "GET", "HEAD" };
            if (get_head.Any(x=>x==method))
            {
                return http;
            }
            if (requestParam.SendData != null)
            {
                http = new ByteArrayContent(requestParam.SendData);
                //http.Headers.ContentType = requestParam.MediaTypeHeaderValue;
                http.Headers.ContentType.CharSet = encoding.WebName;
                return http;
            }
            if (requestParam.Json != null)
            {
                http = new StringContent(requestParam.Json, encoding);
                http.Headers.ContentType = HeaderContentTypeHelper.CreateJson();
                http.Headers.ContentType.CharSet = encoding.WebName;
                return http;
            }
            if (requestParam.PostData != null && requestParam.Files == null)
            {
                http = new FormUrlEncodedContent(RequstsHelper.DicToEnumerableKeyPair(requestParam.PostData));
                http.Headers.ContentType = HeaderContentTypeHelper.CreateFormUrlencoded();
                http.Headers.ContentType.CharSet = encoding.WebName;
                return http;
            }
            if (requestParam.PostData != null && requestParam.Files != null)
            {
                var type = string.Empty;
                http = new ByteArrayContent(RequstsHelper.DicToMsMultiPartFormDataBytes(requestParam.PostData, out type, requestParam.Files, encoding));
                http.Headers.Add("Content-Type", type);
                http.Headers.ContentType.CharSet = encoding.WebName;
                return http;
            }
            return http;

        }


        /// <summary>
        /// 合并头信息
        /// </summary>
        /// <param name="tmpheaders"></param>
        /// <returns></returns>
        private Dictionary<string, string> MerrageCookieFromHeader(HttpRequestHeaders tmpheaders)
        {
            if (tmpheaders.Contains("cookie")) { return CookieHelper.CookieDicFromCookieStr(tmpheaders.GetValues("cookie").ToArray()[0]); }
            return null;
        }

        /// <summary>
        /// 最终合并cookie
        /// </summary>
        /// <param name="tmpheaders"></param>
        /// <param name="tmpcookies"></param>
        /// <returns></returns>
        protected string CreateCookieHeader(HttpRequestHeaders tmpheaders, Dictionary<string, string> tmpcookies)
        {

            var tmpheadersdic = MerrageCookieFromHeader(tmpheaders);
            var tmpcookedic = tmpcookies;
            var value = string.Empty;
            tmpheaders?.Remove("cookie");
            if (tmpheadersdic == null)
            {
                value = string.Join(";", tmpcookedic.Select(x => x.Key + "=" + x.Value));
            }
            else
            {
                foreach (var c in tmpcookedic)
                {
                    tmpheadersdic[c.Key] = c.Value;
                }
                value = string.Join(";", tmpheadersdic.Select(x => x.Key + "=" + x.Value));

            }
            tmpheaders.Add("cookie", value);
            return value;
        }



        /// <summary>  
        /// 获取编码 
        /// </summary>  
        /// <params name="HttpWebResponse">HttpWebResponse对象</params>
        /// <params name="httpResponseByte">HttpWebResponse对象的实体流</params>
        /// <returns>Encoding</returns> 
        protected  Encoding GetEncoding(HttpResponseMessage httpResponseMessage,byte[] httpResponseByte=null)
        {
            var CharacterSet = string.Empty;
            IEnumerable<string> ch;
            var b = httpResponseMessage.Headers.TryGetValues("charSet", out ch);
            if (b) { CharacterSet = ch.ToArray()[0]; }
            Encoding encoding = null;
            if (httpResponseByte != null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(httpResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(CharacterSet))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else
                        {
                            encoding = Encoding.GetEncoding(CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(CharacterSet))
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(CharacterSet);
                    }
                }



            }
            return encoding;
        }
    }
}
