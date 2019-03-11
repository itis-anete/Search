using HtmlAgilityPack;
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
        static Dictionary<int, string> SelectChildNodes(HtmlDocument html)
        {
            Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
            var htmlBody = html.DocumentNode.SelectSingleNode("//body");
            IEnumerable<HtmlNode> childNodes;
            childNodes = htmlBody.DescendantsAndSelf();
            foreach (var node in childNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    indexedInfo.Add(node.OuterHtml.GetHashCode(), node.OuterHtml);
                }
            }
            return indexedInfo;
        }

        //static void GetMetaInformation(HtmlDocument htmlDoc, string value)
        //{
        //    HtmlNode tcNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='" + value + "']");
        //    string description = string.Empty;
        //    if (tcNode != null)
        //    {
        //        HtmlAttribute desc;
        //        desc = tcNode.Attributes["content"];

        //    }
        //}
    }
}
