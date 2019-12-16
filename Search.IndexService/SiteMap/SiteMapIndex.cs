using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Search.IndexService.SiteMap
{
    public class SiteMapIndex
    {
        public List<SiteMapContent> GetSiteMapContentsByIndex(string url)
        {
            var doc = new XmlDocument();

            string content = SiteMapGetter.GetURLContent(url);
            var siteContent = new List<SiteMapContent>();

            try
            {
                doc.LoadXml(content);
            }
            catch { }

            var siteMap = new List<Uri>();

            var uri = new Uri(url);
            var xnList = doc.GetElementsByTagName("sitemap");

            siteMap = SiteMapGetter.GetLinks(xnList, siteMap);

            foreach (var u in siteMap)
            {
                if(u!=null)
                siteContent.Add(SiteMapGetter.GetContent(u.ToString()));
            }

            return siteContent;
        }

        public SiteMapContent GetContentByIndex(string url)
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
