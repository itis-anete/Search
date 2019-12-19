using Nest;
using RailwayResults;
using Search.Core.Extensions;
using System;
using System.Net;
using System.Net.Sockets;

namespace Search.Core.Elasticsearch
{
    public static class ElasticSearchResponseConverter
    {
        public static Result<TValue, HttpStatusCode> ToResultOnFail<TValue, TDocument>(
            ISearchResponse<TDocument> response,
            Func<HttpStatusCode?> customErrorHandler = null
        )
            where TDocument : class
        {
            if (response.OriginalException?.InnerException is SocketException)
                return Result<TValue, HttpStatusCode>.Fail(HttpStatusCode.ServiceUnavailable);

            if (response.ServerError?.Status == HttpStatusCode.NotFound.ToInt())
                return Result<TValue, HttpStatusCode>.Fail(HttpStatusCode.ServiceUnavailable);

            var statusCode = customErrorHandler?.Invoke() ?? HttpStatusCode.InternalServerError;
            return Result<TValue, HttpStatusCode>.Fail(statusCode);
        }
    }
}
