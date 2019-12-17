namespace FSharpSearchService

open RailwayResults;
open Search.Core.Elasticsearch;
open Search.Core.Entities;
open System.Linq;
open System.Net;

type Searcher(client:ElasticSearchClient<Document>,options:ElasticSearchOptions,searchCache:IRequestCache)=
    