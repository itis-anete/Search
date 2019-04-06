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

namespace Search.API
{
    class BirdFormatter
    {

        public static void UnBird(string text)
        {
            List<Tuple<string, string, string>> UnBird = new List<Tuple<string, string, string>>();
            Regex typeHeader = new Regex(@"neb{\w*neck");
            Regex nameHeader = new Regex(@"neck\w* = ");
            Regex valueHeader = new Regex(@" = \w*}tail,");

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
            string neb = "neb{";
            string tail = "}tail,";
            string bird;
            foreach (var prop in objInfo)
            {
                bird = neb + prop.Item2 + "neck" + prop.Item1 + " = " + prop.Item3 + tail;
            }
        }
        public static async void ReadBird(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                string result = await reader.ReadToEndAsync();
            }
        }

        public static async void WriteBird(string fileName, string text)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                await writer.WriteLineAsync(text);
            }
        }

        public static List<Tuple<string, Type, string>> GetProperties(object obj)
        {
            List<Tuple<string, Type, string>> objInfo = new List<Tuple<string, Type, string>>();
            Type t = typeof(object);
            PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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

        //static Dictionary<int, string> JSONIndexer(JsonDocument json)
        //{
        //    Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
            

        //    return indexedInfo;
        //}

        //static Dictionary<int, string> XMLIndexer()
        //{
        //    Dictionary<int, string> indexedInfo = new Dictionary<int, string>();
        //    return indexedInfo;
        //}
    }
}
