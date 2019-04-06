using System.Collections.Generic;

namespace Search.SearchService.Internal
{
    internal class RequestComparer : IEqualityComparer<SearchRequest>
    {
        public bool Equals(SearchRequest x, SearchRequest y)
        {
            return Equals(x.From, y.From)
                && Equals(x.Size, y.Size)
                && Equals(x.Query, y.Query);
        }

        public int GetHashCode(SearchRequest obj)
        {
            return unchecked(
                obj.Query.GetHashCode() * 2081561 +
                obj.From.GetHashCode() * 61583 +
                obj.Size.GetHashCode());
        }
    }
}
