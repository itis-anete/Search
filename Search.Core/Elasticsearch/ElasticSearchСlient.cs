using Nest;
using Search.Core.Entities;
using System;

namespace Search.Core.Elasticsearch
{
    public class ElasticSearchClient
    {
        public event Action<Document, IIndexRequest<Document>, IndexResponse> OnIndex;

        private readonly ElasticClient client;

        public ElasticSearchClient(ElasticSearchOptions options)
        {
            client = new ElasticClient(
                new ConnectionSettings(options.Url)
            );
        }

        public CreateIndexResponse CreateIndex(
            IndexName index,
            Func<CreateIndexDescriptor, ICreateIndexRequest> selector
        )
        {
            return client.Indices.Create(index, selector);
        }

        public void Index(
            Document document,
            Func<IndexDescriptor<Document>, IIndexRequest<Document>> selector
        )
        {
            var response = client.Index(document, selector);

            var desc = new IndexDescriptor<Document>();
            OnIndex?.Invoke(document, selector(desc), response);
        }

        public ExistsResponse IndexExists(Indices indices)
        {
            return client.Indices.Exists(indices);
        }

        public ISearchResponse<Document> Search(Func<SearchDescriptor<Document>, ISearchRequest> selector)
        {
            return client.Search(selector);
        }
    }
}
