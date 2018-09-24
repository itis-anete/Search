using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Search.App
{
    public class Downloader
    {
        private readonly WebClient _client = new WebClient();

        public string DownloadPage(string url)
        {
            var data = _client.DownloadString(url);

            return data;
        }
    }
}
