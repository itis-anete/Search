using Nest;
using Search.Core.Entities;
using System;

namespace Search.Infrastructure
{
    public class ElasticSearchClient
    {
        private Action<DocumentInfo, IIndexRequest<DocumentInfo>, IIndexResponse> _onIndex;
        public event Action<DocumentInfo, IIndexRequest<DocumentInfo>, IIndexResponse> OnIndex
        {
            add { _onIndex += value; }
            remove { _onIndex -= value; }
        }

        private Action<ISearchRequest, ISearchResponse<DocumentInfo>> _onSearch;
        public event Action<ISearchRequest, ISearchResponse<DocumentInfo>> OnSearch
        {
            add { _onSearch += value; }
            remove { _onSearch -= value; }
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
            DocumentInfo document,
            Func<IndexDescriptor<DocumentInfo>, IIndexRequest<DocumentInfo>> selector)
        {
            var response = _client.Index(document, selector);

            var desc = new IndexDescriptor<DocumentInfo>(null, null);
            _onIndex?.Invoke(document, selector(desc), response);
        }

        public IExistsResponse IndexExists(Indices indices)
        {
            return _client.IndexExists(indices);
        }

        public ISearchResponse<DocumentInfo> Search(Func<SearchDescriptor<DocumentInfo>, ISearchRequest> selector)
        {
            var response = _client.Search(selector);

            var desc = new SearchDescriptor<DocumentInfo>();
            _onSearch?.Invoke(selector(desc), response);

            return response;
        }

        private readonly ElasticClient _client;
    }
}
