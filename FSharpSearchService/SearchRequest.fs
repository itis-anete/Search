namespace FSharpSearchService

open System.ComponentModel.DataAnnotations;

type SearchRequest()=

    [<DefaultValue>]
    [<Range(0,2147483647)>]
    val mutable public From: int
    member this.form
        with get()=this.From
        and set(value)=this.From<-value

    [<DefaultValue>]
    [<Range(1,2147483647)>]
    val mutable public Size:int
    member this.size
        with get()=this.Size
        and set(value)=this.Size<-value

    [<DefaultValue>]
    [<MinLength(1)>]
    val mutable public Query:string
    member this.query
        with get()=this.Query
        and set(value)=this.Query<-value