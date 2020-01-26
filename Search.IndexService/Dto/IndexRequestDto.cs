using Search.IndexService.Models;
using System;

namespace Search.IndexService.Dto
{
    public class IndexRequestDto
    {
        public Uri Url { get; set; }
        
        public DateTime CreatedTime { get; set; }

        public IndexRequestStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public int FoundPages { get; set; }

        public int IndexedPages { get; set; }

        public DateTime StartIndexing { get; set; }

        public DateTime FinishIndexing { get; set; }
    }
}
