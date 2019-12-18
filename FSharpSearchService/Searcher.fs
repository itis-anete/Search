namespace FSharpSearchService

open RailwayResults;
open Search.Core.Elasticsearch;
open Search.Core.Entities;
open System;
open System.Net;

type Searcher private()=
    [<DefaultValue>]
    val mutable _client:ElasticSearchClient<Document>
    [<DefaultValue>]
    val mutable _options:ElasticSearchOptions

    member this.Search(request: SearchRequest):Result<SearchResponse, HttpStatusCode> =
        raise (NotImplementedException())
        
    public new(client: ElasticSearchClient<Document>, options: ElasticSearchOptions) as this =
        Searcher()
        then
            this._client<-client
            this._options<-options

        

        
    