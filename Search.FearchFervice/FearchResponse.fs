namespace Search.FearchFervice

open System.Collections.Generic

type FearchResponse() =
    [<DefaultValue>]
    val mutable public Results: IList<FearchResult>
    member this.results
        with get() = this.Results
        and set(value) = this.Results <- value
        

