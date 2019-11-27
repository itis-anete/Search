using Search.Core.Elasticsearch;
using Search.Core.Entities;
using Search.IndexService.Internal;
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
            var html = GetHtml(request.Url);
            var parsedHtml = Parser.HtmlToText.ParseHtml(html);

            var document = new Document()
            {
                Url = request.Url,
                IndexedTime = DateTime.UtcNow,
                Title = parsedHtml.Title,
                Text = parsedHtml.Text
            };

            _client.Index(document, desc => desc
                .Id(document.Url.ToString())
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
                    .Map<Document>(map => map
                        .Properties(properties => properties
                            .Text(ElasticSearchOptions.TitleProperty)
                            .Text(ElasticSearchOptions.TextProperty)
                            .Keyword(ElasticSearchOptions.UrlProperty)
                        ).SourceField(source => source
                            .Excludes(new[] { "text" })
                        )
                    )
                )
            );
        }

        private string GetHtml(Uri url)
        {
            string html;
            using (var client = new HttpClient())
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            using (HttpContent content = response.Content)
                html = content.ReadAsStringAsync().Result;
            
            return html;
        }
    }
}
