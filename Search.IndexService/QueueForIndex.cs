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
            if (CheckQueue(request.Url))
            {
                var indexLink = new IndexRequest()
                {
                    CreatedTime = DateTime.Now,
                    Url = request.Url,
                    Status = IndexRequestStatus.Pending
                };
                
                _client.Index(indexLink, x => x.Id(indexLink.Url.ToString()).Index(_options.DocumentsIndexName));
            }
        }

        public IndexRequest GetIndexElement()
        {//method for return next link for index
            var responseFromElastic = _client.Search(search => search.Index(_options.DocumentsIndexName).
              Query(desc => desc.Match(m => m.Field(x => x.Status == IndexRequestStatus.Pending))));

            var result = responseFromElastic.Documents
                .Select(x => new IndexRequest
                {
                    Url = x.Url,
                    CreatedTime = x.CreatedTime,
                    Status = x.Status
                });
            IndexRequest indexRequest = result.FirstOrDefault(x => x.CreatedTime == result.Min(w => w.CreatedTime));

            if (indexRequest == null)
            {
                return null;
            }
            else
            {
                ChangeStatusElementToInprogress(indexRequest);
                return indexRequest;
            }
        }

        public IEnumerable<IndexRequest> GetAllElementsQueue() 
        {
            var responseFromElastic = _client.Search(search => search.Index(_options.DocumentsIndexName));

            var result = responseFromElastic.Documents
                .Select(x => new IndexRequest
                {
                    Url = x.Url,
                    CreatedTime = x.CreatedTime,
                    Status = x.Status
                });
            return result;
        }

        private void ChangeStatusElementToInprogress(IndexRequest indexRequest)
        {//method for change statua to inprogress 
            indexRequest.Status = IndexRequestStatus.InProgress;
            _client.Index(indexRequest, x => x.Id(indexRequest.Url.ToString()).Index(_options.DocumentsIndexName));
        }

        public void ChangeStatusElementToIndexed(IndexRequest indexRequest) 
        {//method for change status to indexed
            indexRequest.Status = IndexRequestStatus.Indexed;
            _client.Index(indexRequest, x => x.Id(indexRequest.Url.ToString()).Index(_options.DocumentsIndexName));
        }

        public IndexRequest WaitForIndexElement()
        {
            IndexRequest request = null;
            while (request == null)
                request = GetIndexElement();
            return request;
        }

        private bool CheckQueue(Uri url) 
        {
            var responseFromElastic = _client.Search(search => search.Index(_options.DocumentsIndexName).
              Query(desc => desc.Match(m => m.Field(x => x.Url == url&&x.Status!=IndexRequestStatus.Indexed))));
            //var result = responseFromElastic.Documents
            //    .Select(x => new IndexRequest
            //    {
            //        Url = x.Url,
            //        CreatedTime = x.CreatedTime,
            //        Status = x.Status
            //    }).First();
            if (responseFromElastic == null)
            {
                return true;
            }
            else
                return false;
        }

    }
}
