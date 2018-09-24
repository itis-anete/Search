using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Search.App
{
    public class Searcher
    {
        public IEnumerable<int> Find(string htmlPage, string pattern)
        {
            var index = htmlPage.IndexOf(pattern);
            if (index == -1)
                yield break;
            yield return index;
        }
    }
}
