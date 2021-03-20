using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Dynamic;

namespace NetAsyncSpider.Core.Untils
{
    public static class RequstsHelper
    {
        /// <summary>
        /// 对参数编成URL的形式
        /// </summary>
        /// <param name="param"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        internal static string EncodeParams(IEnumerable<KeyValuePair<string, string>> param, Encoding encoding)
        {
            var encodpar = string.Empty;
            if (param == null) { return encodpar; }
            var encode = encoding ?? Encoding.UTF8;
            List<String> pars = new List<String>();
            foreach (KeyValuePair<String, string> par in param)
            {
                pars.Add(System.Web.HttpUtility.UrlEncode(par.Key, encode) + "=" + System.Web.HttpUtility.UrlEncode(par.Value, encode));
            }
            encodpar = String.Join("&", pars);
            return encodpar;
        }

        /// <summary>
        /// Url预处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="encoding"></param>
        internal static string PrepareUrl(string url, IEnumerable<KeyValuePair<string, string>> param, Encoding encoding)
        {
            var Url = string.Empty;
            if (!(url.Contains("http://") || url.Contains("https://")))
            {
                url = "http://" + url;
            }
            if (param == null) { Url = url; return Url; }
            var uri = new Uri(url);
            if (String.IsNullOrWhiteSpace(uri.Query)) { Url = url + "?" + RequstsHelper.EncodeParams(param, encoding); return Url; }
            else { Url = url + "&" + RequstsHelper.EncodeParams(param, encoding); return Url; }
        }


        /// <summary>
        /// 更新头信息
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="headerdic"></param>
        internal static void UpdateRequestHeader(HttpRequestHeaders headers, Dictionary<string, string> headerdic)
        {
            if (headerdic == null) return;
            foreach (var kv in headerdic)
            {
                if (headers.Contains(kv.Key))
                {
                    headers.Remove(kv.Key);
                }
                headers.Add(kv.Key, kv.Value);
            }
        }

        internal static void SetServerCertificateCustomValidationCallback(HttpClientHandler httpClientHandler)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (reqmessage, x509cert, x509chain, sslp) => true;
        }

        /// <summary>
        /// Url预处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="encoding"></param>
        public static string PrepareUrl(string url, Dictionary<string, object> param, Encoding encoding = null)
        {
            var Url = string.Empty;
            if (!(url.Contains("http://") || url.Contains("https://")))
            {
                url = "http://" + url;
            }
            if (param == null) { Url = url; return Url; }
            var uri = new Uri(url);
            var i = string.Join("&", DicToEnumerableKeyPairEncode(param, encoding).Select(x => x.Key + "=" + x.Value));
            if (String.IsNullOrWhiteSpace(uri.Query)) { Url = url + "?" + i; return Url; }
            else { Url = url + "&" + i; return Url; }

        }

        /// <summary>
        /// 对参数编成IEnumerable<KeyValuePair<string, string>>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> DicToEnumerableKeyPairEncode(Dictionary<string, Object> param, Encoding encoding=null)
        {
            var encodpar = string.Empty;
            if (param == null) { throw new Exception("param不能为空"); }
            var encode = encoding ?? Encoding.UTF8;
            List<KeyValuePair<String, string>> pars = new List<KeyValuePair<String, string>>();
            foreach (KeyValuePair<String, Object> par in param)
            {
                if (par.Value.GetType().IsValueType || par.Value.GetType() == typeof(String))
                {
                    pars.Add(new KeyValuePair<string, string>(System.Web.HttpUtility.UrlEncode(par.Key, encode), System.Web.HttpUtility.UrlEncode(par.Value.ToString(), encode)));
                }
                else if (typeof(IEnumerable<string>).IsAssignableFrom(par.Value.GetType()))
                {
                    var tmp = (par.Value as IEnumerable<string>).Select(x =>
                         new KeyValuePair<string, string>(System.Web.HttpUtility.UrlEncode(par.Key, encode), System.Web.HttpUtility.UrlEncode(x, encode))
                    );
                    pars.AddRange(tmp);
                }
                else
                {
                    throw new Exception("目前值只接受值String|IEnumerable<String>的字典");
                }

            }

            return pars;

        }
        /// <summary>
        /// 对参数编成IEnumerable<KeyValuePair<string, string>>
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> DicToEnumerableKeyPair(Dictionary<string, Object> param)
        {
            var encodpar = string.Empty;
            if (param == null) { throw new Exception("param不能为空"); }
            List<KeyValuePair<String, string>> pars = new List<KeyValuePair<String, string>>();
            foreach (KeyValuePair<String, Object> par in param)
            {
                if (par.Value.GetType().IsValueType || par.Value.GetType() == typeof(String))
                {
                    pars.Add(new KeyValuePair<string, string>(par.Key, par.Value.ToString()));
                }
                else if (typeof(IEnumerable<string>).IsAssignableFrom(par.Value.GetType()))
                {
                    var tmp = (par.Value as IEnumerable<string>).Select(x =>
                         new KeyValuePair<string, string>(par.Key, x)
                    );
                    pars.AddRange(tmp);
                }
                else
                {
                    throw new Exception("目前值只接受值String|IEnumerable<String>的字典");
                }

            }

            return pars;

        }

        /// <summary>
        /// 参数字典+文件流转Btye[] post参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="files"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] DicToMsMultiPartFormDataBytes(Dictionary<string, object> param, out string contenttype,List<UploadFile> files, Encoding encoding=null) {
            var multipar = new MsMultiPartFormData(encoding);
            foreach (var i in param) {
                multipar.AddFormField(i.Key, i.Value.ToString());
            }

            if(files!=null)
            foreach (var i in files) {
                multipar.AddFile(i.FieldName, i.FileName, i.Stream, i.ContentType);
            }
            multipar.PrepareFormData();
            contenttype = String.Format("multipart/form-data; boundary={0}", multipar.Boundary); 
            return multipar.GetFormData().ToArray();
        }



    }






    /// <summary>
    /// 常用请求ContentType
    /// </summary>
    public static class HeaderContentTypeHelper {
        public static readonly string Json = "application/json";

        public static readonly string FormUrlencoded = "application/x-www-form-urlencoded";

        public static readonly string Text = "text/plain";

        public static readonly string Html = "text/html";

        public static readonly string Javascript = "application/javascript";

        public static readonly string Xml = "application/xml";

        public static readonly string MultipartFormData = "multipart/form-data";
        public static  MediaTypeHeaderValue CreateJson() => new MediaTypeHeaderValue(Json);

        public static  MediaTypeHeaderValue CreateFormUrlencoded() => new MediaTypeHeaderValue(FormUrlencoded);

        public static  MediaTypeHeaderValue CreateText() => new MediaTypeHeaderValue(Text);

        public static  MediaTypeHeaderValue CreateHtml() => new MediaTypeHeaderValue(Html);

        public static  MediaTypeHeaderValue CreateJavascript() => new MediaTypeHeaderValue(Javascript);

        public static  MediaTypeHeaderValue CreateXml() => new MediaTypeHeaderValue(Xml);

        public static MediaTypeHeaderValue CreateMultipartFormData()=> new MediaTypeHeaderValue(MultipartFormData);

    }
}
