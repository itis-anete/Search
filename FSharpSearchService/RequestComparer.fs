namespace FSharpSearchService

open System.Collections.Generic

type RequestComparer()=
    interface IEqualityComparer<SearchRequest> with
        member this.Equals(x: SearchRequest, y: SearchRequest): bool = 
            (obj.Equals(x.form,y.form)&&obj.Equals(x.Size, y.Size)&&obj.Equals(x.Query, y.Query))
        member this.GetHashCode(obj: SearchRequest): int = 
            obj.query.GetHashCode()*2081561+obj.form.GetHashCode()* 61583+obj.size.GetHashCode()