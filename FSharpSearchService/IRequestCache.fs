namespace FSharpSearchService

open System.Runtime.InteropServices

type IRequestCache=
    interface
        abstract IsCache: SearchRequest->bool

        abstract GetResponse:SearchRequest->SearchResponse

        abstract TryGetResponse:request:SearchRequest * [<Out>] response:SearchResponse->bool

        abstract Add:request:SearchRequest * response:SearchResponse

        abstract Remove:SearchRequest->bool
        
    end