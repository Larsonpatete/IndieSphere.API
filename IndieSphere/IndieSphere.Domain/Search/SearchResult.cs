namespace IndieSphere.Domain.Search;

public class SearchResult<T>
{
    public int? TotalCount { get; set; }       // Total items available
    public int Offset { get; set; }           // Current starting index
    public int Limit { get; set; }            // Max items per page
    public List<T> Results { get; set; }      // Current page results
}
