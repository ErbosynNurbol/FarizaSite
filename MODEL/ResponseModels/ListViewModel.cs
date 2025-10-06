namespace MODEL.ResponseModels;

public class ListViewModel<T>
{
    public int TotalCount { get; set; }
    public List<T> DataList { get; set; }
}