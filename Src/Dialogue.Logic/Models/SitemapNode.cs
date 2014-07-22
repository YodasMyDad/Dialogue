using System;
using System.Collections.Generic;

namespace Dialogue.Logic.Models
{
    public class GoogleSiteMap
    {
        public string MainUrl { get; set; }
        public string Name { get; set; }
        public DateTime LastModified { get; set; }
        public IList<SitemapEntry> Pages { get; set; }
    }
    public class SitemapEntry
    {
        public string Url { get; set; }
        public DateTime LastModified { get; set; }
        public string ChangeFrequency { get; set; }
        public string Priority { get; set; }
    }
}