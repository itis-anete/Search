using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Search.IndexService.SiteMap
{
    public class SiteMapIndex
    {
        public static List<SiteMapContent> GetSiteMapContentsByIndex(string url)
        {
            string content = SiteMapGetter.GetURLContent(url);
            var siteContent = new List<SiteMapContent>();

            var doc = SiteMapGetter.GetContent(url);

            var siteMap = new List<Uri>();
            var uri = new Uri(url);

            var xnList = doc.GetElementsByTagName("sitemap");

            siteMap = SiteMapGetter.GetLinks(xnList, siteMap);

            foreach (var u in siteMap)
            {
                if(u!=null)
                siteContent.Add(SiteMapGetter.GetSiteMapContent(u.ToString()));
            }

            return siteContent;
        }

        public static SiteMapContent GetContentByIndex(string url)
        {
            var uri = new Uri(url);
            var res = new SiteMapContent() { Url = uri};
            
            var contents = GetSiteMapContentsByIndex(url);

            foreach (var c in contents)
            {
                res.Links = c.Links;
            }

            return res;
        }
    }
}
