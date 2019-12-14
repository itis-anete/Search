using Search.IndexService.Models;
using System;
using System.Collections.Generic;

namespace Search.IndexService.Dto
{
    public class IndexRequestDto
    {
        public Uri Url { get; set; }
        
        public DateTime CreatedTime { get; set; }

        public IndexRequestStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public IndexRequestDto()
        {
        }

        public static IndexRequestDto FromModel(IndexRequest model)
        {
            var supportedStatuses = new HashSet<IndexRequestStatus>
            {
                IndexRequestStatus.Pending,
                IndexRequestStatus.InProgress,
                IndexRequestStatus.Indexed,
                IndexRequestStatus.Error
            };
            if (!supportedStatuses.Contains(model.Status))
                throw new NotSupportedException(
                    $"Статус запроса {model.Status} не поддерживается.{Environment.NewLine}" +
                    GetRequestInfoString(model.Url, model.CreatedTime)
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

        private static string GetRequestInfoString(Uri url, DateTime createdTime) =>
            $"URL: {url}{Environment.NewLine}" +
                $"Время создания: {createdTime}";
    }
}
