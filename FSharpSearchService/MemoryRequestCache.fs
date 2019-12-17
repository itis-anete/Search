namespace FSharpSearchService

open Nest;
open Search.Core.Elasticsearch;
open Search.Core.Entities;
open Search.FSharpSearchService.Internal;
open System;
open System.Collections.Concurrent;
open System.Diagnostics;
open System.Linq;
open System.Threading;