using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;

namespace Search.IndexService.SiteMap
{
    /// <summary>
    /// Класс-парсер для robot.txt 
    /// для скачивания sitemap.xml сайта
    /// </summary>
    class Robot
    {
        private static string robotAgent = "Mozilla 5.0; RobsRobot 1.2; www.strictly-software.com;";

        private static string proxyServer = ""; 

        private static WebProxy proxy;

        private static string content = string.Empty;

        private static int statusCode = -1;

        private static string lastError = string.Empty;

        private static ArrayList BlockedUrls = new ArrayList();

        static string getURLContent(string URL)
        {
            Console.WriteLine("Access URL: " + URL);
            Console.WriteLine("Proxy: " + proxyServer);
            Console.WriteLine("User Agent: " + robotAgent);

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

            client.Encoding = System.Text.Encoding.UTF8;

            try
            {
                content = client.DownloadString(URL);
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
            
            if (URL == "/robots.txt")
            {
                return false;
            }
            return true;
        }

        public static void ParseRobotsTxtFile(string URL)
        {

            Uri CurrentURL = new Uri(URL);
            string RobotsTxtFile = "http://" + CurrentURL.Authority + "/robots.txt";

            string FileContents = getURLContent(RobotsTxtFile);
            if (!String.IsNullOrEmpty(FileContents))
            {
                string[] fileLines = FileContents.Split(Environment.NewLine.ToCharArray(), 
                    StringSplitOptions.RemoveEmptyEntries);

                bool ApplyToBot = false;

                foreach (string line in fileLines)
                {
                    Console.WriteLine("Line = " + line);
                    RobotCommand CommandLine = new RobotCommand(line);

                    switch (CommandLine.Command)
                    {
                        case "COMMENT": 
                            break;
                        case "user-agent": 
                            if ((CommandLine.UserAgent.IndexOf("*") >= 0) || (CommandLine.UserAgent.IndexOf(robotAgent) >= 0))
                            {
                                ApplyToBot = true;
                                Console.WriteLine(CommandLine.UserAgent + " - Rules apply");
                            }
                            else
                            {
                                ApplyToBot = false;
                            }
                            break;
                        case "disallow":
                            if (ApplyToBot)
                            {
                                if (CommandLine.Url.Length > 0)
                                {
                                    BlockedUrls.Add(CommandLine.Url.ToLower());
                                    Console.WriteLine("DISALLOW " + CommandLine.Url);
                                }
                                else
                                {
                                    Console.WriteLine("ALLOW ALL URLS - BLANK");
                                }
                            }
                            else
                            {
                                Console.WriteLine("DISALLOW " + CommandLine.Url + " for another user-agent");
                            }
                            break;
                        case "allow":
                            Console.WriteLine("ALLOW: " + CommandLine.Url);
                            break;
                        case "sitemap":
                            Console.WriteLine("SITEMAP: " + CommandLine.Url);
                            break;
                        default:
                            Console.WriteLine("# Unrecognised robots.txt entry [" + line + "]");
                            break;
                    }
                }
            }
        }

    }
}
