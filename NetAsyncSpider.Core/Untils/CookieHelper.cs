using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
namespace NetAsyncSpider.Core.Untils
{
    internal static class CookieHelper
    {
        /// <summary>
        /// dic合并
        /// </summary>
        /// <param name="UpdateCookie"></param>
        /// <param name="tmpcookie"></param>
        public static void MergeCookie(Dictionary<string, string> UpdateCookie, Dictionary<string, string> tmpcookie) {
            foreach (var i in tmpcookie) {
                UpdateCookie[i.Key] = i.Value;
            }
        }
        /// <summary>
        /// cookieidc转字符串
        /// </summary>
        /// <param name="Cookie"></param>
        /// <returns></returns>
        public static string GetCookieString(Dictionary<string, string> Cookie) {
            return string.Join(";", Cookie.Select(x => x.Key + "=" + x.Value));
        }
        /// <summary>
        /// 字符串转cookieidc
        /// </summary>
        /// <param name="cook"></param>
        /// <returns></returns>
        public static Dictionary<string, string> CookieDicFromCookieStr(string cook) {
            var c = new Dictionary<string,string>();
            Regex meta = new Regex("[^;]+");
            Regex tmp = new Regex("([^\\s=]+)=(.*)");
            if (meta.IsMatch(cook))
            {
                foreach (Match x in meta.Matches(cook))
                {
                    if (tmp.IsMatch(x.Groups[0].Value))
                    {
                        var tmp1 = tmp.Match(x.Groups[0].Value);
                        c[tmp1.Groups[1].Value] = tmp1.Groups[2].Value;
                    }
                }
            }
            return c;
        }

        /// <summary>
        /// 从请求头获取cookiedic
        /// </summary>
        /// <param name="httpResponseHeaders"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetCookieFromResponseHeader(HttpResponseHeaders httpResponseHeaders)
        {
            IEnumerable<string> tmp_cookie;
            var ishanscooke = httpResponseHeaders.TryGetValues("Set-Cookie", out tmp_cookie);

            var tmp = new Dictionary<string, string>();
            if (!ishanscooke) return tmp;
            foreach (var i in tmp_cookie)
            {
                var t = i.Split(";");
                var j = t[0].Split("=");

                tmp[j[0]] = j.Length>1?j[1]:string.Empty;

            }
            return tmp;
        }


        /// <summary>  
        /// 从CookieContainer 转cookiedic
        /// </summary>  
        /// <param name="cc">CookieContainer</param>
        public static Dictionary<string, string> GetAllCookies(CookieContainer cc)
        {
            var tmp =new Dictionary<string, string>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, cc, new object[] { });

            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) tmp.Add(c.Name,c.Value);
            }
            return tmp;
        }

    }
}
