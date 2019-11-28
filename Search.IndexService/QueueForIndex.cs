using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Search.IndexService
{
    public class QueueForIndex
    {
        public QueueForIndex(ElasticSearchClient<Document> client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;
        }

        private readonly ElasticSearchClient<Document> _client;
        private readonly ElasticSearchOptions _options;

        public void AddToQueueElement(IndexRequest request)
        {
            //methot for adding element to elastic
            var el = new IndexRequest()
            {
                CreatedTime = DateTime.Now,
                Url = request.Url
            };

            //_client.AddQueueIndex(el, x => x.Id(el.Uri.ToString()).Index(_options.DocumentsIndexName));
        }

        public IndexRequest GetIndexElement()
        {
            return null;//method for return next link for index
        }

    }
}
