using Nest;
using Search.Core.Entities;
using System;

namespace Search.Core.Database
{
    public class ElasticSearchClient
    {
        private Action<Document, IIndexRequest<Document>, IIndexResponse> _onIndex;
        public event Action<Document, IIndexRequest<Document>, IIndexResponse> OnIndex
        {
            add { _onIndex += value; }
            remove { _onIndex -= value; }
        }

        public ElasticSearchClient(ElasticSearchOptions options)
        {
            _client = new ElasticClient(
                new ConnectionSettings(options.Url)
                    .ThrowExceptions()
            );
        }

        public ICreateIndexResponse CreateIndex(
            IndexName index,
            Func<CreateIndexDescriptor, ICreateIndexRequest> selector)
        {
            return _client.CreateIndex(index, selector);
        }

        public void Index(
            Document document,
            Func<IndexDescriptor<Document>, IIndexRequest<Document>> selector)
        {
            var response = _client.Index(document, selector);

            var desc = new IndexDescriptor<Document>(null, null);
            _onIndex?.Invoke(document, selector(desc), response);
        }

        public IExistsResponse IndexExists(Indices indices)
        {
            return _client.IndexExists(indices);
        }

        public ISearchResponse<Document> Search(Func<SearchDescriptor<Document>, ISearchRequest> selector)
        {
            return _client.Search(selector);
        }

        private readonly ElasticClient _client;
    }
}
