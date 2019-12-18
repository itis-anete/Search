namespace FSharpSearchService


open Search.Core.Elasticsearch;
open Search.Core.Entities;
open System;
open System.Collections.Concurrent;
open System.Diagnostics;
open System.Linq;
open System.Threading;

type MemoryRequestCache = 
    [<DefaultValue>]
    val mutable _options:ElasticSearchOptions
    val mutable _maxRecordAge:TimeSpan
    val mutable _ageStopwatch:Stopwatch
    val mutable _ageCheckingTimer:Timer
    val mutable _disposed:bool
    val mutable Comparer:RequestComparer ->RequestComparer
    val mutable _cache:ConcurrentDictionary<SearchRequest, CacheRecord>  
    //_cache = new ConcurrentDictionary<SearchRequest, CacheRecord>(Comparer);

    new(client:ElasticSearchClient<Document>,options:ElasticSearchOptions,maxRecordAge:TimeSpan)as this=
        MemoryRequestCache()
        then
            this._options<-options
            this._maxRecordAge<-maxRecordAge
            client.OnIndex+=ClearOnIndexing
            let ageCheckFrequency=_maxRecordAge/2
            this._ageStopwatch<-Stopwatch.StartNew()
            this._ageCheckingTimer<-new Timer(CheckAgeOfAllRecords,null,ageCheckFrequency,ageCheckFrequency);


    interface IRequestCache with
        member this.Add: request:SearchRequest * response:SearchResponse = 
            _cache[request]=new CacheRecord(response, _ageStopwatch.Elapsed);
        member this.GetResponse(request: SearchRequest): SearchResponse = 
            if(_cache.TryGetValue(request, out var record))then return record.Response;
                else null
        member this.IsCache(request: SearchRequest): bool = 
            CheckRecordAge(request, resetAge: true);
            return _cache.ContainsKey(request);
        member this.Remove(request:SearchRequest) =
            _cache.TryRemove(request, out var record);
        member this.TryGetResponse(request: 'a, response: 'a): bool = 
            raise (System.NotImplementedException())
    interface IDisposable with
        member this.Dispose(): unit = 
            raise (System.NotImplementedException()) 