using System;

/// <summary>
/// Объект - контента Sitemap сайта
/// </summary>
namespace Search.IndexService.SiteMap
{
    public class SiteMapContent
    {
        public Uri Url { get; set; }

        public Uri[] Links { get; set; }
    }

}
