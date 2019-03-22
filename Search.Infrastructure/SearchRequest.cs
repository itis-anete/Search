namespace Search.Infrastructure
{
    public class SearchRequest
    {
        public int From { get; set; }
        public int? Size { get; set; }
        public string Query { get; set; }
    }
}
