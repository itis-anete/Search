using Search.IndexService.Dto;
using System;

namespace Search.IndexService.Models
{
    public abstract class IndexRequest
    {
        public Uri Url { get; }

        public DateTime CreatedTime { get; }

        public abstract IndexRequestStatus Status { get; }

        public IndexRequest(Uri url, DateTime createdTime)
        {
            Url = url;
            CreatedTime = createdTime;
        }

        public static IndexRequest FromDto(IndexRequestDto dto)
        {
            return dto.Status switch
            {
                IndexRequestStatus.Pending => new PendingIndexRequest(dto.Url, dto.CreatedTime),
                IndexRequestStatus.InProgress => new InProgressIndexRequest(dto.Url, dto.CreatedTime),
                IndexRequestStatus.Indexed => new IndexedIndexRequest(dto.Url, dto.CreatedTime),
                IndexRequestStatus.Error => new ErrorIndexRequest(dto.Url, dto.CreatedTime, dto.ErrorMessage),
                _ => throw new NotSupportedException(
                        $"Статус запроса {dto.Status} не поддерживается.{Environment.NewLine}" +
                        GetRequestInfoString(dto.Url, dto.CreatedTime)
                    )
            };
        }

        protected static string GetRequestInfoString(Uri url, DateTime createdTime) =>
            $"URL: {url}{Environment.NewLine}" +
                $"Время создания: {createdTime}";
    }
}
