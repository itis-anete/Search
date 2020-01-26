using Search.IndexService.Models;
using System;

namespace Search.IndexService.Dbo
{
    public class IndexRequestDbo
    {
        public Uri Url { get; set; }
        
        public DateTime CreatedTime { get; set; }

        public IndexRequestStatus Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}
