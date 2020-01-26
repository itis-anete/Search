using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace Search.IndexService.SiteMap
{
    public class SiteMapIndex
    {
        private readonly HttpClient httpClient;

        public SiteMapIndex(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient("Page downloader");
        }

        public async Task<SiteMapContent> GetContentByIndex(Uri url, XmlDocument doc)
        {
            var contents = await GetSiteMapContentsByIndex(doc);
            return new SiteMapContent
            {
                Url = url,
                Links = contents
                    .SelectMany(x => x.Links)
                    .ToArray()
            };
        }

        private async Task<SiteMapContent[]> GetSiteMapContentsByIndex(XmlDocument doc)
        {
            var xnList = doc.GetElementsByTagName("sitemap");
            var siteMaps = GetLinks(xnList);

            var siteContentGettingTasks = siteMaps
                .Select(GetSiteMap)
                .ToArray();
            await Task.WhenAll(siteContentGettingTasks);

            return siteContentGettingTasks
                .Select(task => task.Result)
                .ToArray();
        }

        private async Task<SiteMapContent> GetSiteMap(Uri url)
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
                return await GetContentByIndex(url, doc);

            var xnList = doc.GetElementsByTagName("url");
            var links = GetLinks(xnList).ToArray();

            return new SiteMapContent
            {
                Url = url,
                Links = links
            };
        }

        private static List<Uri> GetLinks(XmlNodeList nodeList)
        {
            var links = new List<Uri>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (XmlNode node in nodeList)
            {
                var locNode = node
                    .Cast<XmlNode>()
                    .FirstOrDefault(child => child.Name == "loc");
                if (locNode != null && Uri.IsWellFormedUriString(locNode.InnerText, UriKind.Absolute))
                    links.Add(new Uri(locNode.InnerText));
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
