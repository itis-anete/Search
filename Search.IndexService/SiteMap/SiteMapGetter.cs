using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Search.IndexService.SiteMap
{
    /// <summary>
    /// Класс получения контента SiteMap
    /// </summary>
    public class SiteMapGetter
    {
        private readonly HttpClient httpClient;
        private readonly SiteMapIndex siteMapIndex;

        public SiteMapGetter(IHttpClientFactory httpClientFactory, SiteMapIndex siteMapIndex)
        {
            httpClient = httpClientFactory.CreateClient("Page downloader");
            this.siteMapIndex = siteMapIndex;
        }

        public async Task<SiteMapContent> GetSiteMap(Uri url)
        {
            var doc = await GetContent(url);

            var rootElement = doc?.DocumentElement?.Name;
            if (rootElement == null)
                return new SiteMapContent
                {
                    Url = url,
                    Links = Array.Empty<Uri>()
                };
            if (rootElement == "sitemapindex")
                return await siteMapIndex.GetContentByIndex(url, doc);

            var xnList = doc.GetElementsByTagName("url");
            var links = GetLinks(xnList).ToArray();

            return new SiteMapContent()
            {
                Url = url,
                Links = links
            };
        }

        private static List<Uri> GetLinks(XmlNodeList nodeList)
        {
            var links = new List<Uri>();
            foreach (XmlNode node in nodeList)
            {
                if (node.InnerText != null)
                {
                    if (Uri.IsWellFormedUriString(node.InnerText, UriKind.Absolute))
                        links.Add(new Uri(node.InnerText));
                }
            }
            return links;
        }

        private async Task<XmlDocument> GetContent(Uri url)
        {
            var content = await GetURLContent(url);
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
                return doc;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GetURLContent(Uri url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch
            {
                return null;
            }
        }
    }
}
