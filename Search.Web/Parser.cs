using HtmlAgilityPack;
using System.Text.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Search.API
{
    public class Parser
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

        static Dictionary<int, string> JSONIndexer(JsonDocument json)
        {
            Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
            

            return indexedInfo;
        }

        static Dictionary<int, string> XMLIndexer()
        {
            Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
            return indexedInfo;
        }


    }
}
