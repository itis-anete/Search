using Nest;
using System;

namespace Search.Core.Database
{
    public class ElasticSearchClient<TModel>
        where TModel : class
    {
        private Action<TModel, IIndexRequest<TModel>, IndexResponse> _onIndex;
        public event Action<TModel, IIndexRequest<TModel>, IndexResponse> OnIndex
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

        public void Index(
            TModel document,
            Func<IndexDescriptor<TModel>, IIndexRequest<TModel>> selector)
        {
            var response = _client.Index(document, selector);

            var desc = new IndexDescriptor<TModel>();
            _onIndex?.Invoke(document, selector(desc), response);
        }

        public ExistsResponse IndexExists(Indices indices)
        {
            return _client.Indices.Exists(indices);
        }

        public ISearchResponse<TModel> Search(Func<SearchDescriptor<TModel>, ISearchRequest> selector)
        {
            return _client.Search(selector);
        }

        private readonly ElasticClient _client;
    }
}
