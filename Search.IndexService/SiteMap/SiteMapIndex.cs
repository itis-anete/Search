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

        public async Task<List<SiteMapContent>> GetSiteMapContentsByIndex(XmlDocument doc)
        {
            var xnList = doc.GetElementsByTagName("sitemap");
            var siteMaps = GetLinks(xnList);

            var siteContent = new List<SiteMapContent>();
            foreach (var siteMap in siteMaps)
                siteContent.Add(await GetSiteMap(siteMap));

            return siteContent;
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
                return await GetContentByIndex(url, doc);

            var xnList = doc.GetElementsByTagName("url");
            var links = GetLinks(xnList);

            var validatedLinks = links
                .Distinct()
                .Except(new[] { url })
                .Where(x => x.Host.EndsWith(url.Host))
                .ToArray();

            return new SiteMapContent()
            {
                Url = url,
                Links = validatedLinks
            };
        }

        public static List<Uri> GetLinks(XmlNodeList nodeList)
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

        public async Task<XmlDocument> GetContent(Uri url)
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

        public async Task<string> GetURLContent(Uri url)
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
