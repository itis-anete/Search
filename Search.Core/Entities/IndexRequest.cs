using System;
using System.ComponentModel.DataAnnotations;

namespace Search.Core.Entities
{
    public class IndexRequest
    {
        [Required]
        public Uri Url { get; set; }

        public DateTime CreatedTime { get; set; }

        public IndexRequestStatus Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}
