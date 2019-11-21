using Search.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Search.IndexService
{
    public class IndexRequest
    {
        public Guid Id { get; set; }

        [Required]
        public Uri Url { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime IndexedTime { get; set; }

        public IndexRequestStatus Status { get; set; }
    }
}
