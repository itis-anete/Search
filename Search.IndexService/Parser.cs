using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Search.Web
{
    class BirdFormatter
    {
        public static void UnBird(string text)
        {
            var UnBird = new List<Tuple<string, string, string>>();
            var typeHeader = new Regex(@"neb{\w*neck");
            var nameHeader = new Regex(@"neck\w* = ");
            var valueHeader = new Regex(@" = \w*}tail,");

            MatchCollection typeMatches = typeHeader.Matches(text);
            MatchCollection nameMatches = nameHeader.Matches(text);
            MatchCollection valueMatches = valueHeader.Matches(text);
            for (int i = 0; i <= typeMatches.Count; i++)
            {
                UnBird.Add(new Tuple<string, string, string>
                     (nameMatches[i].Value.Replace("neb{", "").Replace("neck", ""),
                     typeMatches[i].Value.Replace("neck", "").Replace(" = ", ""),
                     valueMatches[i].Value.Replace(" = ", "").Replace("}tail,", "")));
            }

        }

        public static void ToBird(List<Tuple<string, Type, string>> objInfo)
        {
            var neb = "neb{";
            var tail = "}tail,";
            string bird;
            foreach (var prop in objInfo)
            {
                bird = neb + prop.Item2 + "neck" + prop.Item1 + " = " + prop.Item3 + tail;
            }
        }

        public static async void ReadBird(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                string result = await reader.ReadToEndAsync();
            }
        }

        public static async void WriteBird(string fileName, string text)
        {
            using (var writer = new StreamWriter(fileName, false))
            {
                await writer.WriteLineAsync(text);
            }
        }

        public static List<Tuple<string, Type, string>> GetProperties(object obj)
        {
            var objInfo = new List<Tuple<string, Type, string>>();
            var t = typeof(object);
            var propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in propInfos)
            {
                objInfo.Add(
                    new Tuple<string, Type, string>
                    (prop.Name, prop.PropertyType, (string)prop.GetValue(obj)));
            }
            return objInfo;
        }
    }

    public class Parser
    {
        static Dictionary<int, string> HtmlIndexer(HtmlDocument html)
        {
            var indexedInfo = new Dictionary<int, string>();
            var htmlBody = html.DocumentNode.SelectSingleNode("//body");
            var nodes = htmlBody.DescendantsAndSelf();
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
