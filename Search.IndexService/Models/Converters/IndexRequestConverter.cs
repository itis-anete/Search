using Search.IndexService.Dto;
using System;
using System.Collections.Generic;

namespace Search.IndexService.Models.Converters
{
    public static class IndexRequestConverter
    {
        private static readonly HashSet<IndexRequestStatus> supportedStatuses = new HashSet<IndexRequestStatus>
        {
            IndexRequestStatus.Pending,
            IndexRequestStatus.InProgress,
            IndexRequestStatus.Indexed,
            IndexRequestStatus.Error
        };

        public static IndexRequest ToModel(this IndexRequestDto dto)
        {
            return dto.Status switch
            {
                IndexRequestStatus.Pending => new PendingIndexRequest(dto.Url, dto.CreatedTime),
                IndexRequestStatus.InProgress => new InProgressIndexRequest(dto.Url, dto.CreatedTime),
                IndexRequestStatus.Indexed => new IndexedIndexRequest(dto.Url, dto.CreatedTime),
                IndexRequestStatus.Error => new ErrorIndexRequest(dto.Url, dto.CreatedTime, dto.ErrorMessage),
                _ => throw new NotSupportedException(
                        GetStatusNotSupportedMessage(dto.Url, dto.CreatedTime, dto.Status)
                    )
            };
        }

        public static IndexRequestDto ToDto(this IndexRequest model)
        {
            if (model.Status.IsNotSupported())
                throw new NotSupportedException(
                    GetStatusNotSupportedMessage(model.Url, model.CreatedTime, model.Status)
                );

            var dto = new IndexRequestDto
            {
                Url = model.Url,
                CreatedTime = model.CreatedTime,
                Status = model.Status
            };
            if (model.Status == IndexRequestStatus.Error)
                dto.ErrorMessage = ((ErrorIndexRequest)model).ErrorMessage;
            return dto;
        }

        private static bool IsNotSupported(this IndexRequestStatus status) => !supportedStatuses.Contains(status);

        private static string GetStatusNotSupportedMessage(Uri url, DateTime createdTime, IndexRequestStatus status) =>
            $"Статус запроса {status} не поддерживается.{Environment.NewLine}" +
                $"URL: {url}{Environment.NewLine}" +
                $"Время создания: {createdTime}";
    }
}
