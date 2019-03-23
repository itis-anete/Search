using Nest;
using Search.Core.Entities;
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
                Url = request.Url.ToString(),
                Title = request.Url.ToString(),
                Text = page
            };

            _client.IndexDocument(document);
        }

        private readonly ElasticClient _client;
    }
}
