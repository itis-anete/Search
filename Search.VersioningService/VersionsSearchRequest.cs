using System.ComponentModel.DataAnnotations;

namespace Search.VersioningService
{
    public class VersionsSearchRequest
    {
        [Required]
        [MinLength(1)]
        public string Query { get; set; }
    }
}
