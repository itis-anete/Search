namespace FSharpSearchService

open System

type CacheRecord()=

    public new (response:SearchResponse,createTime:TimeSpan)as newCacheRecord=
        CacheRecord()
        then
            newCacheRecord.response<-response
            newCacheRecord.creationTime<-createTime

    [<DefaultValue>]
    val mutable public Response :SearchResponse
    member this.response
        with get()=this.Response
     
    [<DefaultValue>]
    val mutable public CreationTime:TimeSpan
    member this.creationTime
        with get()=this.CreationTime
        and set(value)=this.CreationTime<-value