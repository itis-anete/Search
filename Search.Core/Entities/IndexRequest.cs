using Search.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Search.IndexService
{
    public class IndexRequest
    {
        [Required]
        public Uri Url { get; set; }

        public DateTime CreatedTime { get; set; }

        public IndexRequestStatus Status { get; set; }
    }
}
