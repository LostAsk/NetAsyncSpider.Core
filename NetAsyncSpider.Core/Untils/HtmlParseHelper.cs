using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NetAsyncSpider.Core.Untils
{
    public static class HtmlParseHelper
    {
        /// <summary>
        /// 解析html
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="func"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static TResult ParseHtmlFromBytes<TResult>(this byte[] bytes,Func<HtmlDocument, TResult> func, Encoding encoding=null) {
            var encode = encoding ?? Encoding.UTF8;
           
            using (var men = new MemoryStream(bytes)) {
                var doc = new HtmlDocument();
                doc.Load(men, encode);
                return func(doc);
            } 
        }

        /// <summary>
        /// 获取html中的链接(获取在HTML文档中声明的链接列表， 获取其他HTML文档的引用链接列表，因为它们是在HTML文档中声明的。)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static (List<string>,List<string>) GetUrlFromBytes<TResult>(this byte[] bytes, Encoding encoding = null)
        {
            return ParseHtmlFromBytes(bytes, (doc) => {
                var link = new DocumentWithLinks(doc);
                return (link.Links, link.References);
            }, encoding);
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async ValueTask SaveContentAsync(this byte[] bytes, String path, CancellationToken cancellationToken = default)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length);
                await fs.FlushAsync();
            }
        }
    }


}
