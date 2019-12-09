using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Collections;

namespace Search.IndexService
{
    /// <summary>
    /// Класс получения контента SiteMap
    /// </summary>
    internal class SiteMapGetter
    {

        public static SiteMapContent GetContent(string url)
        {
            var doc = new XmlDocument();
            string content = getURLContent(url);

            try
            {
                doc.LoadXml(content);
            }
            catch { }

            var links = new List<string>();
            //var priority = new List<string>();
            //var lastModified = new List<string>();
            //var changeFreq = new List<string>();
            var uri = new Uri(url);

            var xnList = doc.GetElementsByTagName("url");

            foreach (XmlNode node in xnList)
            {
                if (node.InnerText != null)
                {
                    links.Add(node.InnerText);
                }
            }

            return new SiteMapContent()
            {
                Url = uri,
                Links = links,
                //Priority = priority,
                //LastModified = lastModified,
                //ChangeFrequency = changeFreq
            };
        }

        private static string robotAgent = "Mozilla 5.0; RobsRobot 1.2; www.strictly-software.com;";

        private static string proxyServer = "";

        private static WebProxy proxy;

        private static string content = string.Empty;

        private static int statusCode = -1;

        private static string lastError = string.Empty;

        private static ArrayList BlockedUrls = new ArrayList();

        internal static string getURLContent(string URL)
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

        public static bool URLIsAllowed(string URL)
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
