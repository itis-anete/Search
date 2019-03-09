using System.Collections.Generic;

namespace Search.Core.Entities
{
    public class DocumentInfo
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public Dictionary<string, int> WordsInTextCount { get; set; }
        public Dictionary<string, int> WordsInTagsCount { get; set; }
        public int WordsTotal { get; set; }
    }
}
