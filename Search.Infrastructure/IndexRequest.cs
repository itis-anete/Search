using System;
using System.ComponentModel.DataAnnotations;

namespace Search.Infrastructure
{
    public class IndexRequest
    {
        [Required]
        public Uri Url { get; set; }
    }
}
