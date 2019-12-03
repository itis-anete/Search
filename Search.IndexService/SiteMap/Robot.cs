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


    }
}
