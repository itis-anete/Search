using System;

namespace Search.Core.Entities
{
    public class DocumentInfo
    {
        public Uri Url { get; set; }
        public DateTime IndexedTime { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
}
