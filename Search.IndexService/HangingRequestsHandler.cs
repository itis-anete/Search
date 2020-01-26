using System.Linq;
using MoreLinq;
using Search.Core.Elasticsearch;
using Search.IndexService.Dto;
using Search.IndexService.Models;
using Search.IndexService.Models.Converters;

namespace Search.IndexService
{
    public class HangingRequestsHandler
    {
        private readonly ElasticSearchClient<IndexRequestDto> _elasticClient;
        private readonly ElasticSearchOptions _elasticOptions;

        public HangingRequestsHandler(
            ElasticSearchClient<IndexRequestDto> elasticClient,
            ElasticSearchOptions elasticOptions)
        {
            _elasticClient = elasticClient;
            _elasticOptions = elasticOptions;
        }
        
        public (bool success, string message) CheckHangingRequests()
        {
            var elasticResponse = _elasticClient.Search(search => search
                .Index(_elasticOptions.RequestsIndexName)
                .Query(desc =>
                    desc.Term(t => t
                        .Field(request => request.Status)
                        .Value(IndexRequestStatus.InProgress)
                    )
                )
            );
            if (!elasticResponse.IsValid)
            {
                if (!elasticResponse.TryGetServerErrorReason(out var errorMessage))
                    errorMessage = "Не удалось установить соединение с Elasticsearch";
                return (false, errorMessage);
            }

            if (!elasticResponse.Documents.Any())
                return (true,
                    "Незавершённых из-за аварийной остановки приложения запросов на индексацию не найдено");
            
            elasticResponse.Documents
                .Select(request => (InProgressIndexRequest)request.ToModel())
                .Select(request => request.SetError(
                    $"Не удалось проиндексировать сайт {request.Url} из-за аварийной остановки приложения"))
                .ForEach(request => _elasticClient.Index(request.ToDto(), x => x
                    .Id(request.Url.ToString())
                    .Index(_elasticOptions.RequestsIndexName))
                );
            return (true,
                "Незавершённые из-за аварийной остановки приложения запросы на индексацию помечены как выполненные с ошибкой");
        }
    }
}