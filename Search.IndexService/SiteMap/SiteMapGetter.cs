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
            try
            {
                doc.LoadXml($"{url}/sitemap.xml");
            }
            catch
            {
                throw new XmlException();
            }

            var links = new List<string>();
            var priority = new List<string>();
            var lastModified = new List<string>();
            var changeFreq = new List<string>();

            var xnList = doc.GetElementsByTagName("url");
            foreach (XmlNode node in xnList)
            {
                if(node["loc"].InnerText != null)
                    links.Add(node["loc"].InnerText);
                
                if(node["priority"].InnerText != null)
                    priority.Add(node["priority"].InnerText);

                if (node["lastmod"].InnerText != null)
                    lastModified.Add(node["lastmod"].InnerText);

                if(node["changefreq"].InnerText != null)
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
