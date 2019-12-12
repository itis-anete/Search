using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Search.IndexService
{
    public class QueueForIndex
    {
        public QueueForIndex(ElasticSearchClient<IndexRequest> client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;

            EnsureIndexInElasticCreated();
        }

        private readonly ElasticSearchClient<IndexRequest> _client;
        private readonly ElasticSearchOptions _options;

        public void AddToQueueElement(Uri url)
        {
            if (CheckQueue(url))
            {
                var indexRequest = new IndexRequest()
                {
                    Url = url,
                    CreatedTime = DateTime.UtcNow,
                    Status = IndexRequestStatus.Pending
                };
                
                _client.Index(indexRequest, x => x
                    .Id(indexRequest.Url.ToString())
                    .Index(_options.RequestsIndexName));
            }
        }

        public IndexRequest GetIndexElement()
        {
            var responseFromElastic = _client.Search(search => search
                .Index(_options.RequestsIndexName)
                .Query(desc => desc
                    .Term(t => t
                        .Field(x => x.Status)
                        .Value(IndexRequestStatus.Pending)
                    )
                )
            );

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
            var responseFromElastic = _client.Search(search => search.Index(_options.RequestsIndexName));

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
            _client.Index(indexRequest, x => x
                .Id(indexRequest.Url.ToString())
                .Index(_options.RequestsIndexName)
            );
        }

        public void ChangeStatusElementToIndexed(IndexRequest indexRequest) 
        {//method for change status to indexed
            indexRequest.Status = IndexRequestStatus.Indexed;
            _client.Index(indexRequest, x => x
                .Id(indexRequest.Url.ToString())
                .Index(_options.RequestsIndexName)
            );
        }

        public IndexRequest WaitForIndexElement()
        {
            IndexRequest request = GetIndexElement();
            while (request == null)
            {
                Thread.Sleep(250);
                request = GetIndexElement();
            }
            return request;
        }

        private bool CheckQueue(Uri url) 
        {
            var responseFromElastic = _client.Search(search => search
                .Index(_options.RequestsIndexName)
                .Query(desc => 
                    desc.Term(t => t
                        .Field(x => x.Url)
                        .Value(url)
                    )
                    &&
                    !desc.Term(t => t
                        .Field(x => x.Status)
                        .Value(IndexRequestStatus.Indexed)
                    )
                )
            );

            return !responseFromElastic.Documents.Any();
        }

        private void EnsureIndexInElasticCreated()
        {
            var response = _client.IndexExists(_options.RequestsIndexName);
            if (response.Exists)
                return;

            _client.CreateIndex(_options.RequestsIndexName, index => index
                .Mappings(ms => ms
                    .Map<IndexRequest>(m => m
                        .Properties(ps => ps
                            .Keyword(k => k.Name(x => x.Url))
                            .Date(d => d.Name(x => x.CreatedTime))
                        )
                    )
                )
            );
        }
    }
}
