using Search.Core.Entities;

namespace Search.Infrastructure
{
    public interface ISearchDatabase
    {
        SearchResponse Search(SearchRequest request);
        void Add(DocumentInfo document);
    }
}
