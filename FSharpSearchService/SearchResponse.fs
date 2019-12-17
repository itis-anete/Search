namespace FSharpSearchService

open System.Collections.Generic

type SearchResponse()=
    [<DefaultValue>]
    val mutable public Results :IList<SearchResult>
    member this.results
        with get()=this.Results
        and set(value)=this.Results <- value
        

