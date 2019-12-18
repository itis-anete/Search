namespace FSharpSearchService


open Search.Core.Elasticsearch;
open Search.Core.Entities;
open System;
open System.Collections.Concurrent;
open System.Diagnostics;
open System.Linq;
open System.Threading;

type MemoryRequestCache private()= 
    [<DefaultValue>]
    val mutable _options:ElasticSearchOptions
    [<DefaultValue>]
    val mutable _maxRecordAge:TimeSpan
    [<DefaultValue>]
    val mutable _ageStopwatch:Stopwatch
    [<DefaultValue>]
    val mutable _ageCheckingTimer:Timer
    [<DefaultValue>]
    val mutable _disposed:bool
    [<DefaultValue>]
    val mutable Comparer:RequestComparer ->RequestComparer
    [<DefaultValue>]
    val mutable _cache:ConcurrentDictionary<SearchRequest, CacheRecord>  
    //_cache = new ConcurrentDictionary<SearchRequest, CacheRecord>(Comparer);

    new(client:ElasticSearchClient<Document>,options:ElasticSearchOptions,maxRecordAge:TimeSpan)as this=
        MemoryRequestCache()
        then
            this._options<-options
            this._maxRecordAge<-maxRecordAge
            client.add_OnIndex(ClearOnIndexing,)
            let ageCheckFrequency=maxRecordAge / 2
            this._ageStopwatch<-Stopwatch.StartNew()
            this._ageCheckingTimer<-new Timer(CheckAgeOfAllRecords(),null,ageCheckFrequency,ageCheckFrequency);


    interface IRequestCache with
        member this.Add: request:SearchRequest * response:SearchResponse = 
            this._cache.AddOrUpdate(request,new CacheRecord(response, _ageStopwatch.Elapsed);
        member this.GetResponse(request: SearchRequest): SearchResponse = 
            if(this._cache.TryGetValue(request, out var record))then return record.Response;
                else null
        member this.IsCache(request: SearchRequest): bool = 
             CheckRecordAge(request, resetAge: true);
             this._cache.ContainsKey(request);
        member this.Remove(request:SearchRequest):bool =
            this._cache.TryRemove(request, out var record)
            true
        member this.TryGetResponse(request:SearchRequest,response: SearchResponse): bool = 
            raise (System.NotImplementedException())
    interface IDisposable with
        member this.Dispose(): unit = 
            raise (System.NotImplementedException()) 