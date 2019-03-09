using System.Collections.Generic;

namespace Search.Infrastructure
{
    public class Searcher
    {
        public IEnumerable<string> Search(string query)
        {
            return new[] { "google.com" };
        }
    }
}
