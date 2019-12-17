namespace FSharpSearchService

open System.Collections.Generic

type RequestComparer()=
    interface IEqualityComparer<SearchRequest> with

        member Equals(x:SearchRequest,y:SearchRequest)=
            