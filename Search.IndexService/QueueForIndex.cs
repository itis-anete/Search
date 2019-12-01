using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Search.IndexService
{
    public class QueueForIndex
    {
        public QueueForIndex(ElasticSearchClient<IndexRequest> client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;
        }

        private readonly ElasticSearchClient<IndexRequest> _client;
        private readonly ElasticSearchOptions _options;

        public void AddToQueueElement(IndexRequest request)
        {
            //methot for adding element to elastic
            var indexLink = new IndexRequest()
            {
                CreatedTime = DateTime.Now,
                Url = request.Url,
                Status = IndexRequestStatus.Pending                
            };

            _client.Index(indexLink, x => x.Id(indexLink.Url.ToString()).Index(_options.DocumentsIndexName));
        }

        public IndexRequest GetIndexElement()
        {//method for return next link for index
            var responseFromElastic = _client.Search(search => search.Index(_options.DocumentsIndexName).
              Query(desc => desc.Match(m => m.Field(x => x.Status==IndexRequestStatus.Pending))));


            var result = responseFromElastic.Documents
                .Select(x => new IndexRequest
                {
                    Url = x.Url,
                    CreatedTime = x.CreatedTime,
                    Status = x.Status
                }).OrderBy(s=>s.CreatedTime).First();
            if (result == null)
            {
                return null;
            }
            else  return result;
        }


    }
}
