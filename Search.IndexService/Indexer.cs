using Search.Infrastructure;
using System;

namespace Search.IndexService
{
    public class Indexer
    {
        public Indexer(ElasticSearchDatabase database)
        {
            _database = database;
        }

        public void Index(IndexRequest request)
        {
            request.Document.IndexedTime = DateTime.UtcNow;
            _database.Add(request.Document);
        }

        private readonly ElasticSearchDatabase _database;
    }
}
