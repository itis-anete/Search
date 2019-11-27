using Nest;
using RailwayResults;
using System;
using System.Net;
using System.Net.Sockets;

namespace Search.Core.Elasticsearch
{
    public static class ElasticSearchResponseConverter
    {
        public static Result<TValue, HttpStatusCode> ToResultOnFail<TValue, TDocument>(
            ISearchResponse<TDocument> response,
            Func<HttpStatusCode?> customErrorHandler
        )
            where TDocument : class
        {
            if (response.OriginalException.InnerException is SocketException)
                return Result<TValue, HttpStatusCode>.Fail(HttpStatusCode.ServiceUnavailable);

            var statusCode = customErrorHandler() ?? HttpStatusCode.InternalServerError;
            return Result<TValue, HttpStatusCode>.Fail(statusCode);
        }
    }
}
