namespace IndieSphere.Domain.Search;

public class SearchFilters
{
    public int? MinPopularity { get; set; }
    public int? MaxPopularity { get; set; }
    // Future filters can be added here
    public string Genre { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int? MinTempo { get; set; }
    public int? MaxTempo { get; set; }
    // etc.
}
