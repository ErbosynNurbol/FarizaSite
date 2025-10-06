using MODEL.FormatModels;

namespace MODEL.RequestModels;

public class FilterModel
{
    public int Id { get; set; }
    
    public int Range { get; set; }
    public int PageSize { get; set; }
    public int PageOffset { get; set; }
    
    public string SearchTerm { get; set; }
    public string SortBy { get; set; }
    public List<byte> StatusList { get; set; }
    
    public double Lat { get; set; }
    public double Lng { get; set; }
    // public PointModel Start { get; set; }
    // public PointModel End { get; set; }
}