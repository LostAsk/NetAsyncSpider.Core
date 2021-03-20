using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NetAsyncSpider.Core.Untils
{
    /// <summary> 
    /// 表示需要呈现链接文件的文档，如图像或CSS文件，并指向其他HTML文档.<br></br>
    /// https://blog.csdn.net/WuLex/article/details/78752732
    /// </summary>
    public class DocumentWithLinks
    {
        private List<string> _links;
        private List<string> _references;
        private HtmlDocument _doc;

        /// <summary>
        /// 创建一个DocumentWithLinkedFiles的实例。
        /// </summary>
        /// <param name="doc">输入HTML文件。不可能为null。</param>
        public DocumentWithLinks(HtmlDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            _doc = doc;
            GetLinks();
            GetReferences();
        }

        private void GetLinks()
        {
            _links = new List<string>();
            HtmlNodeCollection atts = _doc.DocumentNode.SelectNodes("//*[@background or @lowsrc or @src or @href]");
            if (atts == null)
                return;

            foreach (HtmlNode n in atts)
            {
                ParseLink(n, "background");
                ParseLink(n, "href");
                ParseLink(n, "src");
                ParseLink(n, "lowsrc");
            }
        }

        private void GetReferences()
        {
            _references = new List<string>();
            HtmlNodeCollection hrefs = _doc.DocumentNode.SelectNodes("//a[@href]");
            if (hrefs == null)
                return;

            foreach (HtmlNode href in hrefs)
            {
                _references.Add(href.Attributes["href"].Value);
            }
        }


        private void ParseLink(HtmlNode node, string name)
        {
            HtmlAttribute att = node.Attributes[name];
            if (att == null)
                return;

            //如果name = href，我们只对<link>标签感兴趣
            if ((name == "href") && (node.Name != "link"))
                return;

            _links.Add(att.Value);
        }

        /// <summary>
        /// 获取在HTML文档中声明的链接列表
        /// </summary>
        public List<string> Links
        {
            get
            {
                return _links;
            }
        }

        /// <summary>
        /// 获取其他HTML文档的引用链接列表，因为它们是在HTML文档中声明的。
        /// </summary>
        public List<string> References
        {
            get
            {
                return _references;
            }
        }
    }
}
