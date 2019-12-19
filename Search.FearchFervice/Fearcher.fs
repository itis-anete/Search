namespace Search.FearchFervice

open Nest;
open RailwayResults;
open Search.Core.Elasticsearch;
open Search.Core.Entities;
open System;
open System.Linq;
open System.Net;

type Fearcher private() =
    [<DefaultValue>]
    val mutable _client: ElasticSearchClient<Document>
    [<DefaultValue>]
    val mutable _options: ElasticSearchOptions

    member this.Search(request: Search.FearchFervice.FearchRequest): Result<FearchResponse, HttpStatusCode> =
        let inline (!>) (x: ^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)
        let matchings = [
            new Func<QueryContainerDescriptor<Document>, QueryContainer>(fun desc ->
                desc.Match(fun m ->
                    m.Field(fun x -> x.Title).Query(request.Query) :> IMatchQuery
                )
            );
            new Func<QueryContainerDescriptor<Document>, QueryContainer>(fun desc ->
                desc.Match(fun m ->
                    m.Field(fun x -> x.Text).Query(request.Query) :> IMatchQuery
                )
            )
        ]
        let responseFromElastic = this._client.Search(fun search ->
            search
                .Index(!> Indices.Index(this._options.DocumentsIndexName))
                .From(request.From)
                .Size(request.Size)
                .Query(fun desc ->
                    desc.Bool(fun b ->
                        b.Should(matchings) :> IBoolQuery
                    )
                ) :> ISearchRequest
            )
        if not responseFromElastic.IsValid then
            ElasticSearchResponseConverter.ToResultOnFail<FearchResponse>(responseFromElastic)
        else
            Result<FearchResponse, HttpStatusCode>.Success(
                new FearchResponse(
                    Results = responseFromElastic.Documents
                        .Select(fun document ->
                            new FearchResult(Url = document.Url, Title = document.Title)
                        )
                        .ToList()
                    )
                );
        
    public new(client: ElasticSearchClient<Document>, options: ElasticSearchOptions) as this =
        Fearcher()
        then
            this._client<-client
            this._options<-options

        

        
    