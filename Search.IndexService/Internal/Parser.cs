using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RailwayResults;

namespace Search.IndexService.Internal
{
    internal static class Parser
    {
        public static class HtmlToText
        {
            public static Result<ParsedHtml, string> ParseHtml(string html, Uri url)
            {
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    if (doc.DocumentNode.Element("html") == null)
                        return Result<ParsedHtml, string>.Fail(doc.ParseErrors.First().Reason);
                    
                    var text = ConvertDoc(doc);
                    DeleteSymbols(doc, text);
                    
                    return Result<ParsedHtml, string>.Success(new ParsedHtml
                    {
                        Title = GetTitle(doc),
                        Text = DeleteExcessWhitespaces(text),
                        Links = GetLinks(html, url)
                    });
                }
                catch (Exception error)
                {
                    return Result<ParsedHtml, string>.Fail(error.ToString());
                }
            }
            
            private static string GetTitle(HtmlDocument doc)
            {
                return doc?.DocumentNode?.SelectSingleNode("//title")?.InnerText ?? "";
            }

            private static string ConvertDoc(HtmlDocument doc)
            {
                using (var sw = new StringWriter())
                {
                    ConvertTo(doc.DocumentNode, sw);
                    sw.Flush();
                    return sw.ToString();
                }
            }

            private static void DeleteSymbols(HtmlDocument doc, string text)
            {
                var sb = new StringBuilder(text);
                foreach (HtmlTextNode node in doc.DocumentNode.SelectNodes("//text()"))
                {
                    if (node.Text.Contains("google - analytics.com / ga.js")
                        | node.Text.Contains("&")
                        | node.Text.Contains(">")
                        | node.Text.Contains("<")
                        | node.Text.Contains("[")
                        | node.Text.CompareTo("\n") == 0
                        | node.Text.Contains("{"))
                        continue;
                    else
                        sb.AppendLine(node.Text);
                }
            }
            private static List<Uri> GetLinks(string htmlText, Uri baseUrl)
            {
                var parser = new HtmlParser();
                var document = parser.ParseDocument(htmlText);
                var href = new List<Uri>();
                foreach (var element in document.QuerySelectorAll("a"))
                {
                    var el = element.GetAttribute("href");
                    if (el != null)
                    {
                        var hrefAttribute = element.GetAttribute("href");
                        if (
                            Uri.TryCreate(hrefAttribute, UriKind.Absolute, out var link)
                            || Uri.TryCreate(baseUrl, hrefAttribute, out link)
                        )
                        {
                            if (link.Scheme != Uri.UriSchemeHttp && link.Scheme != Uri.UriSchemeHttps)
                                continue;
                            if (link.Scheme != baseUrl.Scheme)
                            {
                                var strLink = link.ToString();
                                link = new Uri(
                                    baseUrl.Scheme +
                                        ':' +
                                        strLink.Substring(
                                            strLink.IndexOf(':') + 1
                                        )
                                );
                            }
                            href.Add(link);
                        }
                    }
                }
                return href;
            }

            private static void ConvertContentTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
            {
                foreach (HtmlNode subnode in node.ChildNodes)
                {
                    ConvertTo(subnode, outText, textInfo);
                }
            }

            private static void ConvertTo(HtmlNode node, TextWriter outText)
            {
                ConvertTo(node, outText, new PreceedingDomTextInfo(false));
            }

            private static void ConvertTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
            {
                string html;
                switch (node.NodeType)
                {
                    case HtmlNodeType.Comment:
                        break;
                    case HtmlNodeType.Document:
                        ConvertContentTo(node, outText, textInfo);
                        break;
                    case HtmlNodeType.Text:
                        string parentName = node.ParentNode.Name;
                        if ((parentName == "script") || (parentName == "style"))
                        {
                            break;
                        }
                        html = ((HtmlTextNode)node).Text;
                        if (HtmlNode.IsOverlappedClosingElement(html))
                        {
                            break;
                        }
                        if (html.Length == 0)
                        {
                            break;
                        }
                        if (!textInfo.WritePrecedingWhiteSpace || textInfo.LastCharWasSpace)
                        {
                            html = html.TrimStart();
                            if (html.Length == 0) { break; }
                            textInfo.IsFirstTextOfDocWritten.Value = textInfo.WritePrecedingWhiteSpace = true;
                        }
                        outText.Write(HtmlEntity.DeEntitize(Regex.Replace(html.TrimEnd(), @"\s{2,}", " ")));
                        if (textInfo.LastCharWasSpace = char.IsWhiteSpace(html[html.Length - 1]))
                        {
                            outText.Write(' ');
                        }
                        break;
                    case HtmlNodeType.Element:
                        string endElementString = null;
                        bool isInline;
                        bool skip = false;
                        int listIndex = 0;
                        switch (node.Name)
                        {
                            case "nav":
                            case "title":
                                skip = true;
                                isInline = false;
                                break;
                            case "body":
                            case "section":
                            case "article":
                            case "aside":
                            case "h1":
                            case "h2":
                            case "header":
                            case "footer":
                            case "address":
                            case "main":
                            case "div":
                            case "a":
                            case "p": 
                                if (textInfo.IsFirstTextOfDocWritten)
                                {
                                    outText.Write("\r\n");
                                }
                                endElementString = "\r\n";
                                isInline = false;
                                break;
                            case "br":
                                outText.Write("\r\n");
                                skip = true;
                                textInfo.WritePrecedingWhiteSpace = false;
                                isInline = true;
                                break;
                            case "li":
                                if (textInfo.ListIndex > 0)
                                {
                                    outText.Write("\r\n{0}.\t", textInfo.ListIndex++);
                                }
                                else
                                {
                                    outText.Write("\r\n*\t");
                                }
                                isInline = false;
                                break;
                            case "ol":
                                listIndex = 1;
                                goto case "ul";
                            case "ul": 
                                endElementString = "\r\n";
                                isInline = false;
                                break;
                            case "img": 
                                if (node.Attributes.Contains("alt"))
                                {
                                    outText.Write(' ' + node.Attributes["alt"].Value);
                                    endElementString = " ";
                                }
                                isInline = true;
                                break;
                            default:
                                isInline = true;
                                break;
                        }
                        if (!skip && node.HasChildNodes)
                        {
                            ConvertContentTo(node, outText, isInline ? textInfo : new PreceedingDomTextInfo(textInfo.IsFirstTextOfDocWritten) { ListIndex = listIndex });
                        }
                        if (endElementString != null)
                        {
                            outText.Write(endElementString);
                        }
                        break;
                }
            }

            private static string DeleteExcessWhitespaces(string str)
            {
                const int minWhitespaceGapLength = 1;
                var sb = new StringBuilder(str);

                var currentIndex = 0;
                var firstWhitespaceIndex = default(int?);
                while (currentIndex < sb.Length)
                {
                    var currentSymbol = sb[currentIndex];
                    if (char.IsWhiteSpace(currentSymbol) || char.IsControl(currentSymbol))
                    {
                        if (firstWhitespaceIndex == null)
                            firstWhitespaceIndex = currentIndex;
                    }
                    else
                    {
                        if (firstWhitespaceIndex != null)
                        {
                            var whitespaceGapLength = currentIndex - firstWhitespaceIndex.Value;
                            if (whitespaceGapLength > minWhitespaceGapLength)
                            {
                                sb.Remove(firstWhitespaceIndex.Value, whitespaceGapLength - minWhitespaceGapLength);
                                currentIndex = firstWhitespaceIndex.Value + minWhitespaceGapLength + 1;
                            }

                            firstWhitespaceIndex = null;
                        }
                    }
                    ++currentIndex;
                }
                if (firstWhitespaceIndex != null)
                    sb.Remove(firstWhitespaceIndex.Value, sb.Length - firstWhitespaceIndex.Value);

                return sb.ToString();
            }
        }

        private class PreceedingDomTextInfo
        {
            public PreceedingDomTextInfo(BoolWrapper isFirstTextOfDocWritten)
            {
                IsFirstTextOfDocWritten = isFirstTextOfDocWritten;
            }

            public bool WritePrecedingWhiteSpace { get; set; }
            public bool LastCharWasSpace { get; set; }
            public readonly BoolWrapper IsFirstTextOfDocWritten;
            public int ListIndex { get; set; }
        }

        private class BoolWrapper
        {
            public BoolWrapper() { }

            public bool Value { get; set; }

            public static implicit operator bool(BoolWrapper boolWrapper)
            {
                return boolWrapper.Value;
            }

            public static implicit operator BoolWrapper(bool boolWrapper)
            {
                return new BoolWrapper { Value = boolWrapper };
            }
        }
    }
}
