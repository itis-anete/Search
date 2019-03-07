using System;
using System.Collections.Generic;

namespace Search.Database.Entities
{
    public class DocumentInfo
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public Dictionary<string, int> CountOfWordsInText { get; set; }
        public Dictionary<string, int> CountOfWordsInTags { get; set; }
        public int TotalCountOfWords { get; set; }
    }
}
