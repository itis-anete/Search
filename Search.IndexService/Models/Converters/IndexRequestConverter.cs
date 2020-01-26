using Search.IndexService.Dbo;
using System;
using System.Collections.Generic;

namespace Search.IndexService.Models.Converters
{
    public static class IndexRequestConverter
    {
        private static readonly HashSet<IndexRequestStatus> SupportedStatuses = new HashSet<IndexRequestStatus>
        {
            IndexRequestStatus.Pending,
            IndexRequestStatus.InProgress,
            IndexRequestStatus.Indexed,
            IndexRequestStatus.Error
        };

        public static IndexRequest ToModel(this IndexRequestDbo dbo)
        {
            return dbo.Status switch
            {
                IndexRequestStatus.Pending => new PendingIndexRequest(dbo.Url, dbo.CreatedTime),
                IndexRequestStatus.InProgress => new InProgressIndexRequest(dbo.Url, dbo.CreatedTime, dbo.StartIndexingTime, dbo.IndexedPagesCount, dbo.FoundPagesCount),
                IndexRequestStatus.Indexed => new IndexedIndexRequest(dbo.Url, dbo.CreatedTime, dbo.StartIndexingTime, dbo.IndexedPagesCount, dbo.EndIndexingTime),
                IndexRequestStatus.Error => new ErrorIndexRequest(dbo.Url, dbo.CreatedTime, dbo.ErrorMessage),
                _ => throw new NotSupportedException(
                        GetStatusNotSupportedMessage(dbo.Url, dbo.CreatedTime, dbo.Status)
                    )
            };
        }

        public static IndexRequestDbo ToDbo(this IndexRequest model)
        {
            if (model.Status.IsNotSupported())
                throw new NotSupportedException(
                    GetStatusNotSupportedMessage(model.Url, model.CreatedTime, model.Status)
                );

            var dbo = new IndexRequestDbo
            {
                Url = model.Url,
                CreatedTime = model.CreatedTime,
                Status = model.Status
            };
            switch (model.Status)
            {
                case IndexRequestStatus.InProgress:
                {
                    var inProgressModel = (InProgressIndexRequest)model;
                    dbo.StartIndexingTime = inProgressModel.StartIndexingTime;
                    dbo.IndexedPagesCount = inProgressModel.IndexedPagesCount;
                    dbo.FoundPagesCount = inProgressModel.FoundPagesCount;
                    break;
                }
                case IndexRequestStatus.Indexed:
                {
                    var indexedModel = (IndexedIndexRequest)model;
                    dbo.StartIndexingTime = indexedModel.StartIndexingTime;
                    dbo.IndexedPagesCount = indexedModel.IndexedPagesCount;
                    dbo.EndIndexingTime = indexedModel.EndIndexingTime;
                    break;
                }
                case IndexRequestStatus.Error:
                {
                    var errorModel = (ErrorIndexRequest)model;
                    dbo.ErrorMessage = errorModel.ErrorMessage;
                    break;
                }
            }
            return dbo;
        }

        private static bool IsNotSupported(this IndexRequestStatus status) => !SupportedStatuses.Contains(status);

        private static string GetStatusNotSupportedMessage(Uri url, DateTime createdTime, IndexRequestStatus status) =>
            $"Статус запроса {status} не поддерживается.{Environment.NewLine}" +
                $"URL: {url}{Environment.NewLine}" +
                $"Время создания: {createdTime}";
    }
}
