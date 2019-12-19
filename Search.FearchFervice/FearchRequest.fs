namespace Search.FearchFervice

open System;
open System.ComponentModel.DataAnnotations;

type FearchRequest() =
    [<DefaultValue>]
    [<Range(0,2147483647)>]
    val mutable public From: Nullable<int>
    member this.form
        with get() = this.From
        and set(value) = this.From <- value

    [<DefaultValue>]
    [<Range(1,2147483647)>]
    val mutable public Size: Nullable<int>
    member this.size
        with get() = this.Size
        and set(value) = this.Size <- value

    [<DefaultValue>]
    [<MinLength(1)>]
    val mutable public Query: string
    member this.query
        with get() = this.Query
        and set(value) = this.Query <- value