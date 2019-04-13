using System.ComponentModel.DataAnnotations;

namespace Search.SearchService
{
    public class SearchRequest
    {
        [Range(0, int.MaxValue)]
        public int? From { get; set; }

        [Range(1, int.MaxValue)]
        public int? Size { get; set; }

        [Required]
        [MinLength(1)]
        public string Query { get; set; }
    }
}
