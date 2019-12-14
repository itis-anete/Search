using System;

namespace Search.IndexService.Models
{
    public class ErrorIndexRequest : IndexRequest
    {
        public string ErrorMessage { get; }

        public ErrorIndexRequest(Uri url, DateTime createdTime, string errorMessage)
            : base(url, createdTime)
        {
            if (string.IsNullOrEmpty(errorMessage))
                throw new ArgumentException(
                    $"Не указан текст ошибки при попытке создать запрос на индексацию, завершившийся с ошибкой.{Environment.NewLine}" +
                        GetRequestInfoString(url, createdTime),
                    nameof(errorMessage)
                );
            ErrorMessage = errorMessage;
        }

        public override IndexRequestStatus Status => IndexRequestStatus.Error;
    }
}
