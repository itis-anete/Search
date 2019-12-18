using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Collections;
using System.Linq;
using Search.IndexService.SiteMap;

namespace Search.IndexService
{
    /// <summary>
    /// Класс получения контента SiteMap
    /// </summary>
    public class SiteMapGetter
    {
        public static XmlDocument GetContent(string url)
        {
            var doc = new XmlDocument();
            string content = GetURLContent(url);

            try
            {
                doc.LoadXml(content);
            }
            catch { }
            return doc;
        }
        public static SiteMapContent GetSiteMapContent(string url)
        {
            var doc = GetContent(url);

            var links = new List<Uri>();
            var uri = new Uri(url);
            XmlNodeList xnList;

            try
            {
                xnList = doc.GetElementsByTagName("url");
                links = GetLinks(xnList, links);

                var urlObject = new Uri(url);
                var validatedLinks = links
                    .Distinct()
                    .Except(new[] { urlObject })
                    .Where(x => x.Host.EndsWith(urlObject.Host))
                    .ToArray();

                return new SiteMapContent()
                {
                    Url = uri,
                    Links = validatedLinks
                };
            }
            catch
            {
                return SiteMapIndex.GetContentByIndex(url);
            }            
        }

        public static List<Uri> GetLinks(XmlNodeList nodeList, List<Uri> list)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node.InnerText != null)
                {
                    if (Uri.IsWellFormedUriString(node.InnerText, UriKind.Absolute))
                        list.Add(new Uri(node.InnerText));
                }
            }
            return list;
        }

        private static string robotAgent = "Mozilla 5.0; RobsRobot 1.2; www.strictly-software.com;";

        private static string proxyServer = "";

        private static WebProxy proxy;

        private static string content = string.Empty;

        private static int statusCode = -1;

        private static string lastError = string.Empty;

        private static ArrayList BlockedUrls = new ArrayList();

        public static string GetURLContent(string URL)
        {
            content = string.Empty;
            lastError = "";
            statusCode = -1;

            WebClient client = new WebClient();

            if (!String.IsNullOrEmpty(proxyServer))
            {
                if (proxy == null)
                {
                    proxy = new WebProxy(proxyServer, true);
                    client.Proxy = proxy;
                }
            }

            client.Headers["User-Agent"] = robotAgent;

            client.Encoding = Encoding.UTF8;

            try
            {
                using (client)
                {
                    content = client.DownloadString(URL);
                }
                statusCode = 200;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    lastError = "Bad Domain Name";
                }
                else if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse HTTPResponse = (HttpWebResponse)ex.Response;

                    statusCode = (int)HTTPResponse.StatusCode;

                    if (HTTPResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        lastError = "Page Not Found";
                    }
                    else if (HTTPResponse.StatusCode == HttpStatusCode.Forbidden)
                    {
                        lastError = "Access Forbidden";
                    }
                    else if (HTTPResponse.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        lastError = "Server Error";
                    }
                    else if (HTTPResponse.StatusCode == HttpStatusCode.Gone)
                    {
                        lastError = "Page No Longer Available";
                    }
                    else
                    {
                        lastError = HTTPResponse.StatusDescription;
                    }
                }
                else
                {
                    lastError = "Error: " + ex.ToString();
                }
            }
            catch (Exception ex)
            {
                lastError = "Error: " + ex.ToString();
            }
            finally
            {
                client.Dispose();
            }

            if (!String.IsNullOrEmpty(lastError))
            {
                Console.WriteLine(statusCode.ToString() + ": " + lastError);
            }

            return content;
        }

        public bool URLIsAllowed(string URL)
        {
            if (BlockedUrls.Count == 0)
                return true;

            Uri checkURL = new Uri(URL);
            URL = checkURL.AbsolutePath.ToLower();

            if (URL == "/sitemap.xml")
            {
                return false;
            }
            return true;
        }

    }
}
