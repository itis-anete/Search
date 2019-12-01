using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Search.IndexService
{
    public class QueueForReindex
    {
        private int ReindexTime { get; set; }
        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;
        public QueueForReindex(ElasticSearchClient<Document> client,
            ElasticSearchOptions options)
        {
            ReindexTime = -7;
            _client = client;
            _options = options;
        }

        public Document GetReindexDocument() 
        {
            var responseFromElastic = _client.Search(search => search
                .Index(_options.DocumentsIndexName)
                .Query(desc => desc
                    .Match(match => match
                        .Field(x => x.IndexedTime == DateTime.Now.AddDays(ReindexTime)))));
            if (responseFromElastic == null)
            {
                return null;
            }
            else
                return responseFromElastic.Documents
                .Select(doc => new Document
                {
                    Url = doc.Url,
                    IndexedTime = doc.IndexedTime,
                    Text = doc.Text,
                    Title = doc.Title
                }).First();
        }

        public void ChangeIndexDate(Document document) 
        {

        }
    }
}
