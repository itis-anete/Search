using Search.Core.Entities;
using Search.Infrastructure;
using System.Net;

namespace Search.IndexService
{
    public class Indexer
    {
        public Indexer(ISearchDatabase searchDatabase)
        {
            _searchDatabase = searchDatabase;
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

            _searchDatabase.Add(document);
        }

        private readonly ISearchDatabase _searchDatabase;
    }
}
