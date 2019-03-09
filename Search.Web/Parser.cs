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
        static void GetMetaInformation(HtmlDocument htmlDoc, string value)
        {
            HtmlNode tcNode = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='" + value + "']");
            string description = string.Empty;
            if (tcNode != null)
            {
                HtmlAttribute desc;
                desc = tcNode.Attributes["content"];

            }
        }

        static Dictionary<int, string> SelectChildNodes(HtmlDocument html)
        {
            Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
            var htmlBody = html.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection childNodes = htmlBody.ChildNodes;
            foreach (var node in childNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    indexedInfo.Add(node.OuterHtml.GetHashCode(), node.OuterHtml);
                }
            }
            return indexedInfo;
        }
        //public void GetAllNodes(HtmlDocument html)
        //{
        //    HtmlDocument htmlDoc = new HtmlDocument();
        //    string strHtml = html.ToString();

        //    HtmlNodeCollection nodes = htmlDoc.DocumentNode.ChildNodes;

        //    foreach (HtmlNode node in nodes)
        //    {
        //        if (node.Name.ToLower() == "p")
        //        {
        //            Paragraph newPPara = new Paragraph();
        //            foreach (HtmlNode childNode in node.ChildNodes)
        //            {
        //                InterNode(childNode, ref newPPara);
        //            }
        //            richTextBlock.Blocks.Add(newPPara);
        //        }
        //    }
        //}
        //public bool InterNode(HtmlNode htmlNode, ref Paragraph originalPar)
        //{
        //    string htmlNodeName = htmlNode.Name.ToLower();

        //    List<string> nodeAttList = new List<string>();
        //    HtmlNode parentNode = htmlNode.ParentNode;
        //    while (parentNode != null)
        //    {
        //        nodeAttList.Add(parentNode.Name);
        //        parentNode = parentNode.ParentNode;
        //    } //we need to get it multiple types, because it could be b(old) and i(talic) at the same time.

        //    Inline newRun = new Run();
        //    foreach (string noteAttStr in nodeAttList) //with this we can set all the attributes to the inline
        //    {
        //        switch (noteAttStr)
        //        {
        //            case ("b"):
        //            case ("strong"):
        //                {
        //                    newRun.FontWeight = FontWeights.Bold;
        //                    break;
        //                }
        //            case ("i"):
        //            case ("em"):
        //                {
        //                    newRun.FontStyle = FontStyle.Italic;
        //                    break;
        //                }
        //        }
        //    }

        //    if (htmlNodeName == "#text") //the #text means that its a text node. Like <i><#text/></i>. Thanks @HungCao
        //    {
        //        ((Run)newRun).Text = htmlNode.InnerText;
        //    }
        //    else //if it is not a #text, don't load its innertext, as it's another node and it will always have a #text node as a child (if it has any text)
        //    {
        //        foreach (HtmlNode childNode in htmlNode.ChildNodes)
        //        {
        //            InterNode(childNode, ref originalPar);
        //        }
        //    }
        //    return true;
        //}

    }
}
