using Nest;
using Search.Core.Entities;
using System;
using System.Net;

namespace Search.IndexService
{
    public class Indexer
    {
        public Indexer(ElasticClient client)
        {
            _client = client;
        }

        public void Index(IndexRequest request)
        {
            var page = "";
            using (var client = new WebClient())
                page = client.DownloadString(request.Url);

            var document = new DocumentInfo
            {
                Url = request.Url,
                IndexedTime = DateTime.UtcNow,
                Title = request.Url.Host + request.Url.PathAndQuery,
                Text = page
            };

            _client.IndexDocument(document);
        }

        private readonly ElasticClient _client;
    }
}
