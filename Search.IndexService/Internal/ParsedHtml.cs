using System.Collections.Generic;

namespace Search.IndexService.Internal
{
    internal class ParsedHtml
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public List<string> Links { get; set; }
    }
}
