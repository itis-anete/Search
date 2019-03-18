using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.Infrastructure.Implementation
{
    public class MemorySearchCache : ISearchCache
    {
        public void Add(SearchRequest request, SearchResponse response)
        {
            
        }

        public SearchResponse GetResponse(SearchRequest request)
        {
            return null;
        }

        public bool IsCached(SearchRequest request)
        {
            return false;
        }

        public void Remove(SearchRequest request)
        {
            
        }

        public bool TryGetResponse(SearchRequest request, out SearchResponse response)
        {
            response = null;
            return false;
        }
    }
}
