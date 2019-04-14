using System;

namespace Search.Core.Entities
{
    public class DocumentInfo
    {
        public Uri Url { get; set; }
        public DateTime IndexedTime { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DocumentInfo(url url)
        {
            var doc = GetHtml(url);
            Url = url;
            Title = doc.DocumentNode.SelectSingleNode("//title");
            Text = ConvertToPlainText(doc).Replace(Header, "");
        }
    }
}
