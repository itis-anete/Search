using MoreLinq;
using RailwayResults;
using Search.Core.Elasticsearch;
using Search.IndexService.Dbo;
using Search.IndexService.Models;
using Search.IndexService.Models.Converters;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Search.IndexService
{
    public class QueueForIndex
    {
        private readonly ElasticSearchClient<IndexRequestDbo> _client;
        private readonly ElasticSearchOptions _options;

        public QueueForIndex(ElasticSearchClient<IndexRequestDbo> client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;

            EnsureIndexInElasticCreated();
        }

        public Result<HttpStatusCode> AddToQueueElement(Uri url)
        {
            var checkResult = CheckQueue(url);
            if (checkResult.IsFailure)
                return checkResult;

            if (checkResult.Value)
            {
                var dto = new PendingIndexRequest(url, DateTime.UtcNow).ToDbo();
                _client.Index(dto, x => x
                    .Id(dto.Url.ToString())
                    .Index(_options.RequestsIndexName));
            }
            return Result<HttpStatusCode>.Success();
        }

        public PendingIndexRequest GetIndexElement()
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

            var requestDto = responseFromElastic.Documents
                .MinBy(x => x.CreatedTime)
                .FirstOrDefault();
            return requestDto == null
                ? null
                : (PendingIndexRequest)requestDto.ToModel();
        }

        public Result<IndexRequest[], HttpStatusCode> GetAllElementsQueue() 
        {
            var countResponse = _client.GetCount(_options.RequestsIndexName);
            if (!countResponse.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<IndexRequest[]>(countResponse);

            var responseFromElastic = _client.Search(search => search
                .Index(_options.RequestsIndexName)
                .Size((int)countResponse.Count)
                .Sort(s => s.Descending(x => x.CreatedTime))
            );
            if (!responseFromElastic.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<IndexRequest[]>(responseFromElastic);

            var results = responseFromElastic.Documents
                .Select(x => x.ToModel())
                .ToArray();
            return Result<IndexRequest[], HttpStatusCode>.Success(results);
        }

        public void Update(IndexRequest indexRequest)
        {
            var dto = indexRequest.ToDbo();
            _client.Index(dto, x => x
                .Id(dto.Url.ToString())
                .Index(_options.RequestsIndexName)
            );
        }

        public async Task UpdateAsync(IndexRequest indexRequest)
        {
            var dto = indexRequest.ToDbo();
            await _client.IndexAsync(dto, x => x
                .Id(dto.Url.ToString())
                .Index(_options.RequestsIndexName)
            );
        }

        public PendingIndexRequest WaitForIndexElement()
        {
            var request = GetIndexElement();
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
                    (
                        desc.Term(t => t
                            .Field(x => x.Status)
                            .Value(IndexRequestStatus.Pending)
                        )
                        ||
                        desc.Term(t => t
                            .Field(x => x.Status)
                            .Value(IndexRequestStatus.InProgress)
                        )
                    )
                )
            );
            if (!responseFromElastic.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<bool>(responseFromElastic);

            return Result<bool, HttpStatusCode>.Success(!responseFromElastic.Documents.Any());
        }

        private void EnsureIndexInElasticCreated()
        {
            var response = _client.IndexExists(_options.RequestsIndexName);
            if (response.Exists)
                return;

            _client.CreateIndex(_options.RequestsIndexName, index => index
                .Mappings(ms => ms
                    .Map<IndexRequestDbo>(m => m
                        .Properties(ps => ps
                            .Keyword(k => k.Name(x => x.Url))
                            .Date(d => d.Name(x => x.CreatedTime))
                            .Text(t => t.Name(x => x.ErrorMessage))
                        )
                    )
                )
            );
        }
    }
}
