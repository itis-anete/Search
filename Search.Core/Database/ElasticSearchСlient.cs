using Nest;
using Search.Core.Entities;
using System;

namespace Search.Core.Database
{
    public class ElasticSearchClient
    {
        private Action<Document, IIndexRequest<Document>, IndexResponse> _onIndex;
        public event Action<Document, IIndexRequest<Document>, IndexResponse> OnIndex
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

        public CreateIndexResponse CreateIndex(
            IndexName index,
            Func<CreateIndexDescriptor, ICreateIndexRequest> selector)
        {
            return _client.Indices.Create(index, selector);
        }

        public IGetResponse<Document> Get(
            DocumentPath<Document> document,
            Func<GetDescriptor<Document>, IGetRequest> selector = null)
        {
            return _client.Get(document, selector);
        }

        public void Index(
            Document document,
            Func<IndexDescriptor<Document>, IIndexRequest<Document>> selector)
        {
            var response = _client.Index(document, selector);

            var desc = new IndexDescriptor<Document>(null, null);
            _onIndex?.Invoke(document, selector(desc), response);
        }

        public ExistsResponse IndexExists(Indices indices)
        {
            return _client.Indices.Exists(indices);
        }

        public ISearchResponse<Document> Search(Func<SearchDescriptor<Document>, ISearchRequest> selector)
        {
            return _client.Search(selector);
        }

        private readonly ElasticClient _client;
    }
}
