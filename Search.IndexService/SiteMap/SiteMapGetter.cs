using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Search.IndexService
{
    /// <summary>
    /// Класс получения контента SiteMap
    /// </summary>
    public class SiteMapGetter
    {
        private readonly HttpClient httpClient;
        
        public SiteMapGetter(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient("Page downloader");
        }

        public async Task<SiteMapContent> GetSiteMap(Uri url)
        {
            var siteMapUrl = new Uri(url, "/sitemap.xml");
            var content = await GetURLContent(siteMapUrl);

            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(content);
            }
            catch { }

            var xnList = doc.GetElementsByTagName("url");

            var links = new List<Uri>();
            foreach (XmlNode node in xnList)
            {
                if (node.InnerText != null)
                {
                    if (Uri.IsWellFormedUriString(node.InnerText, UriKind.Absolute))
                        links.Add(new Uri(node.InnerText));
                }
            }

            var validatedLinks = links
                .Distinct()
                .Except(new[] { url })
                .Where(x => x.Host.EndsWith(url.Host))
                .ToArray();
            return new SiteMapContent()
            {
                Url = siteMapUrl,
                Links = validatedLinks
            };
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
