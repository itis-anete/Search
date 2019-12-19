namespace Search.IndexHelpers

open Microsoft.Extensions.Options;

type PagesPerSiteLimiter private() =
    [<DefaultValue>]
    val mutable private pagesPerSiteLimit: int
    member public this.PagesPerSiteLimit
        with get() = this.pagesPerSiteLimit

    public new(options: IOptionsMonitor<PagesPerSiteLimiterOptions>) as this =
        PagesPerSiteLimiter()
        then
            this.pagesPerSiteLimit <- options.CurrentValue.PagesPerSiteLimit

    member this.IsLimitReached(pagesCount: int) =
        pagesCount > this.pagesPerSiteLimit