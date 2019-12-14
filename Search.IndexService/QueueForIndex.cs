using MoreLinq;
using RailwayResults;
using Search.Core.Elasticsearch;
using Search.IndexService.Dto;
using Search.IndexService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Search.IndexService
{
    public class QueueForIndex
    {
        public QueueForIndex(ElasticSearchClient<IndexRequestDto> client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;

            EnsureIndexInElasticCreated();
        }

        private readonly ElasticSearchClient<IndexRequestDto> _client;
        private readonly ElasticSearchOptions _options;

        public Result<HttpStatusCode> AddToQueueElement(Uri url)
        {
            var checkResult = CheckQueue(url);
            if (checkResult.IsFailure)
                return checkResult;

            if (checkResult.Value)
            {
                var indexRequest = new PendingIndexRequest(url, DateTime.UtcNow);
                var indexRequestDto = IndexRequestDto.FromModel(indexRequest);
                
                _client.Index(indexRequestDto, x => x
                    .Id(indexRequest.Url.ToString())
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
                : (PendingIndexRequest)IndexRequest.FromDto(requestDto);
        }

        public Result<IndexRequest[], HttpStatusCode> GetAllElementsQueue() 
        {
            var responseFromElastic = _client.Search(search => search.Index(_options.RequestsIndexName));
            if (!responseFromElastic.IsValid)
                return ElasticSearchResponseConverter.ToResultOnFail<IndexRequest[], IndexRequestDto>(responseFromElastic);

            var results = responseFromElastic.Documents
                .Select(x => IndexRequest.FromDto(x))
                .ToArray();
            return Result<IndexRequest[], HttpStatusCode>.Success(results);
        }

        public void Update(IndexRequest indexRequest)
        {
            var dto = IndexRequestDto.FromModel(indexRequest);
            _client.Index(dto, x => x
                .Id(dto.Url.ToString())
                .Index(_options.RequestsIndexName)
            );
        }

        public void SetError(Uri url, string errorMessage)
        {
            var response = _client.Get(
                url.ToString(),
                _options.RequestsIndexName
            );
            if (!response.IsValid)
                return;

            response.Source.Status = IndexRequestStatus.Error;
            response.Source.ErrorMessage = errorMessage;
            _client.Index(response.Source, x => x
                .Id(response.Source.Url.ToString())
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
                return ElasticSearchResponseConverter.ToResultOnFail<bool, IndexRequestDto>(responseFromElastic);

            return Result<bool, HttpStatusCode>.Success(!responseFromElastic.Documents.Any());
        }

        private void EnsureIndexInElasticCreated()
        {
            var response = _client.IndexExists(_options.RequestsIndexName);
            if (response.Exists)
                return;

            _client.CreateIndex(_options.RequestsIndexName, index => index
                .Mappings(ms => ms
                    .Map<IndexRequestDto>(m => m
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
