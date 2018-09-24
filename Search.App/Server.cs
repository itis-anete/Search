using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.App
{
    public class Server
    {
        private readonly Downloader _downloader = new Downloader();
        private readonly Searcher _searcher = new Searcher();

        public static Server Instance { get; } = new Server();
        private Server() { }

        public IEnumerable<int> Search(string url, string pattern)
        {
            var page = _downloader.DownloadPage(url);
            return _searcher.Find(page, pattern);
        }
    }
}
