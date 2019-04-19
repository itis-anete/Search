using HtmlAgilityPack;
using Search.IndexService;
using Search.Core.Entities;
using Search.Infrastructure;
using System;
using System.Net.Http;

namespace Search.IndexService
{
    public class Indexer
    {
        public Indexer(ElasticSearchClient client, ElasticSearchOptions options)
        {
            _client = client;
            _options = options;

            EnsureIndexCreated();
        }

        public void Index(IndexRequest request)
        {
            HtmlDocument html = GetHtml(request.Url);
            var text = Parser.HtmlToText.ConvertDoc(html);

            DocumentInfo docInf = new DocumentInfo(request.Url, html, text);
            _client.Index(docInf, desc => desc
                .Id(docInf.Url.ToString())
                .Index(_options.DocumentsIndexName));
        }

        private readonly ElasticSearchClient _client;
        private readonly ElasticSearchOptions _options;

        private void EnsureIndexCreated()
        {
            var response = _client.IndexExists(_options.DocumentsIndexName);
            if (response.Exists)
                return;
            
            _client.CreateIndex(_options.DocumentsIndexName, index => index
                .Settings(ElasticSearchOptions.AnalysisSettings)
                .Mappings(mappings => mappings
                    .Map<DocumentInfo>(map => map
                        .Properties(properties => properties
                            .Text(ElasticSearchOptions.TitleProperty)
                            .Text(ElasticSearchOptions.TextProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }

        public HtmlDocument GetHtml(Uri url)
        {
            string result;
            using (var client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            using (HttpContent content = response.Content)
                result = content.ReadAsStringAsync().Result;

            var document = new HtmlDocument();
            document.LoadHtml(result);
            return document;
        }

    }
}
