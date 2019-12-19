namespace Search.IndexHelpers

type PagesPerSiteLimiterOptions() =
    [<DefaultValue>]
    val mutable private pagesPerSiteLimit: int
    member this.PagesPerSiteLimit
        with get() = this.pagesPerSiteLimit
        and set(value) = this.pagesPerSiteLimit <- value