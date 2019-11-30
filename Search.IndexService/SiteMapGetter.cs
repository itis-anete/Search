using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Search.IndexService
{
    /// <summary>
    /// Класс получения контента SiteMap
    /// </summary>
    internal class SiteMapGetter
    {
        public static SiteMapContent GetContetn(Uri url)
        {
            var doc = new XmlDocument();
            doc.LoadXml($"{url}/sitemap.xml");

            var links = new List<string>();
            var priority = new List<string>();
            var lastModified = new List<string>();
            var changeFreq = new List<string>();

            var xnList = doc.GetElementsByTagName("url");
            foreach (XmlNode node in xnList)
            {
                links.Add(node["loc"].InnerText);
                priority.Add(node["priority"].InnerText);
                lastModified.Add(node["lastmod"].InnerText);
                changeFreq.Add(node["changefreq"].InnerText);
            }

            return new SiteMapContent()
            {
                Url = url,
                Links = links,
                Priority = priority,
                LastModified = lastModified,
                ChangeFrequency = changeFreq
            };
        }

    }
}
