using HtmlAgilityPack;
using System.Text.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;


namespace Search.Web.Indexing
{
    public class HtmlParser
    {
        static Dictionary<int, string> HtmlIndexer(HtmlDocument html)
        {
            Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
            var htmlBody = html.DocumentNode.SelectSingleNode("//body");
            IEnumerable<HtmlNode> nodes;
            nodes = htmlBody.DescendantsAndSelf();
            foreach (var node in nodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    indexedInfo.Add(node.OuterHtml.GetHashCode(), node.OuterHtml);
                }
            }
            return indexedInfo;
        }
    }
}
