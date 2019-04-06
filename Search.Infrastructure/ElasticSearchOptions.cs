using Nest;
using Search.Core.Entities;
using System;

namespace Search.Infrastructure
{
    public class ElasticSearchOptions
    {
        public Uri Url { get; set; }
        public string DocumentsIndexName { get; set; } = "documents_index";

        public bool EnableVersioning { get; set; }
        public string VersionsIndexName { get; set; } = "versions_index";

        public static ITextProperty TitleProperty(TextPropertyDescriptor<DocumentInfo> property) => property
            .Name(x => x.Title)
            .Analyzer("english_russian")
            .Boost(3);

        public static ITextProperty TextProperty(TextPropertyDescriptor<DocumentInfo> property) => property
            .Name(x => x.Text)
            .Analyzer("english_russian")
            .Store();

        public static IDateProperty IndexedTimeProperty(DatePropertyDescriptor<DocumentInfo> property) => property
            .Name(x => x.IndexedTime);

        public static IPromise<IIndexSettings> AnalysisSettings(IndexSettingsDescriptor settings) => settings
            .Analysis(analysis => analysis
                .TokenFilters(filters => filters
                    .Stop("stop", stop => stop.StopWords("_english_", "_russian_"))
                    .Stemmer("english_stemmer", stemmer => stemmer.Language("english"))
                    .Stemmer("english_possessive_stemmer", stemmer => stemmer.Language("possessive_english"))
                    .Stemmer("russian_stemmer", stemmer => stemmer.Language("russian"))
                )
                .Analyzers(analyzers => analyzers
                    .Custom("english_russian", analyzer => analyzer
                        .Tokenizer("standard")
                        .Filters("lowercase", "stop", "english_stemmer", "english_possessive_stemmer", "russian_stemmer")
                    )
                )
            );
    }
}
