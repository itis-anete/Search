using System;
using HtmlAgilityPack;

//using Search.IndexService;
namespace Search.Core.Entities
{
    public class DocumentInfo
    {
        public Uri Url { get; set; }
        public DateTime IndexedTime { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DocumentInfo(Uri url, HtmlDocument doc, string text)
        {
            Url = url;
            IndexedTime = DateTime.UtcNow;
            Title = doc.DocumentNode.SelectSingleNode("//title").ToString();
            Text = text.Replace(Title, ""); 
        }
    }
}
