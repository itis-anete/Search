using Search.Core.Database;
using Search.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Search.IndexService
{
    public class QueueForIndex
    {
        public QueueForIndex(ElasticSearchClient client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;
        }

        private readonly ElasticSearchClient _client;
        private readonly ElasticSearchOptions _options;

        public void AddToQueueElement(IndexRequest request)
        {
            //methot for adding element to elastic
            var el = new QueueModel()
            {
                RequestTime = DateTime.Now,
                Uri = request.Url
            };

            _client.AddQueueIndex(el, x => x.Id(el.Uri.ToString()).Index(_options.DocumentsIndexName));
        }

        public QueueModel GetIndexElement()
        {
            return null;//method for return next link for index
        }

    }
}
