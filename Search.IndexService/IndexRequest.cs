using System;
using System.ComponentModel.DataAnnotations;

namespace Search.IndexService
{
    public class IndexRequest
    {
        [Required]
        public Url Url { get; set; }

        public Core.Entities.DocumentInfo Document { get; set; }
        public string Header { get; set; }
        public string Text { get; set; }
        IndexRequest(url url)
        {
            var doc = GetHtml(url);
            Url = url;
            Document = Core.Entites.DocumentInfo(doc);
            Header = doc.DocumentNode.SelectSingleNode("//title");
            Text = ConvertToPlainText(doc).Replace(Header, "");     
        }

    }
}
