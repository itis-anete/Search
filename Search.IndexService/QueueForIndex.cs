using RailwayResults;
using Search.Core.Elasticsearch;
using Search.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public Result<HttpStatusCode> AddToQueueElement(Uri url)
        {
            var checkResult = CheckQueue(url);
            if (checkResult.IsFailure)
                return checkResult;

            if (checkResult.Value)
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
            return Result<HttpStatusCode>.Success();
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
            if (!responseFromElastic.IsValid)
                return null;

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
                UpdateStatus(indexRequest.Url, IndexRequestStatus.InProgress);
                return indexRequest;
            }
        }

        public Result<IEnumerable<IndexRequest>, HttpStatusCode> GetAllElementsQueue() 
        {
            var responseFromElastic = _client.Search(search => search.Index(_options.RequestsIndexName));
            if (!responseFromElastic.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<IEnumerable<IndexRequest>, IndexRequest>(responseFromElastic);

            var result = responseFromElastic.Documents
                .Select(x => new IndexRequest
                {
                    Url = x.Url,
                    CreatedTime = x.CreatedTime,
                    Status = x.Status
                });
            return Result<IEnumerable<IndexRequest>, HttpStatusCode>.Success(result);
        }

        public void UpdateStatus(Uri url, IndexRequestStatus newStatus)
        {
            var response = _client.Get(
                url.ToString(),
                _options.RequestsIndexName
            );
            if (!response.IsValid)
                return;

            response.Source.Status = newStatus;
            _client.Index(response.Source, x => x
                .Id(response.Source.Url.ToString())
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

        private Result<bool, HttpStatusCode> CheckQueue(Uri url) 
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
            if (!responseFromElastic.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<bool, IndexRequest>(responseFromElastic);

            return Result<bool, HttpStatusCode>.Success(!responseFromElastic.Documents.Any());
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
