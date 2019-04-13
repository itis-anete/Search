using System;
using System.ComponentModel.DataAnnotations;

namespace Search.IndexService
{
    public class IndexRequest
    {
        //[Required]
        //public Uri Url { get; set; }

        public Core.Entities.DocumentInfo Document { get; set; }
    }
}
