using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace Search.Core.Elasticsearch
{
    public class ElasticSearchClient<TModel>
        where TModel : class
    {
        public event Action<TModel, IIndexRequest<TModel>, IndexResponse> OnIndex;

        public ElasticSearchClient(ElasticSearchOptions options)
        {
            _client = new ElasticClient(
                new ConnectionSettings(options.Url)
            );
        }

        public CreateIndexResponse CreateIndex(
            IndexName index,
            Func<CreateIndexDescriptor, ICreateIndexRequest> selector)
        {
            return _client.Indices.Create(index, selector);
        }

        public void Delete(
            DocumentPath<TModel> documentPath,
            IndexName indexName
        )
        {
            _client.Delete(documentPath, x => x.Index(indexName));
        }

        public void DeleteMany(
            IEnumerable<string> documentPaths,
            IndexName indexName
        )
        {
            var bulkRequest = new BulkRequest
            {
                Operations = new BulkOperationsCollection<IBulkOperation>(
                    documentPaths.Select(path => new BulkDeleteOperation<TModel>(path)
                    {
                        Index = indexName
                    })
                )
            };
            _client.Bulk(bulkRequest);
        }

        public GetResponse<TModel> Get(
            DocumentPath<TModel> documentPath,
            IndexName indexName
        )
        {
            return _client.Get(documentPath, x => x.Index(indexName));
        }

        public CountResponse GetCount(IndexName indexName)
        {
            return _client.Count<TModel>(count => count.Index(indexName));
        }

        public void Index(
            TModel document,
            Func<IndexDescriptor<TModel>, IIndexRequest<TModel>> selector)
        {
            var response = _client.Index(document, selector);

            var desc = new IndexDescriptor<TModel>();
            OnIndex?.Invoke(document, selector(desc), response);
        }

        public async Task IndexAsync(
            TModel document,
            Func<IndexDescriptor<TModel>, IIndexRequest<TModel>> selector)
        {
            var response = await _client.IndexAsync(document, selector);

            var desc = new IndexDescriptor<TModel>();
            OnIndex?.Invoke(document, selector(desc), response);
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
