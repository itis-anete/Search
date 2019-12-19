namespace Search.FearchFervice

open System

type FearchResult() =
    [<DefaultValue>]
    val mutable public Url: Uri
    member this.url
        with get() = this.Url
        and set(value) = this.Url <- value

    [<DefaultValue>]
    val mutable public Title: string
    member this.title
        with get() = this.Title
        and set(value) = this.Title <- value

