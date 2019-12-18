namespace FSharpSearchService

open RailwayResults;
open Search.Core.Elasticsearch;
open Search.Core.Entities;
open System.Linq;
open System.Net;

type Searcher private()=
     [<DefaultValue>]
     val mutable _client:ElasticSearchClient<Document>
     [<DefaultValue>]
     val mutable _options:ElasticSearchOptions
     [<DefaultValue>]
     val mutable _searchCache:IRequestCache

     member Search(request:SearchRequest):Result<SearchResponse, HttpStatusCode> =
        
    

     new(client:ElasticSearchClient<Document>,options:ElasticSearchOptions,searchCache:IRequestCache) as this=
     Searcher()
     then
        this._client<-client
        this._options<-options
        this._searchCache<-searchCache

        

        
    