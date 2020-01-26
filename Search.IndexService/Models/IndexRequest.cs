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

        protected static string GetRequestInfoString(Uri url, DateTime createdTime) =>
            $"URL: {url}{Environment.NewLine}" +
                $"Время создания: {createdTime}";
    }
}
