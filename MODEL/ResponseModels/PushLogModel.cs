namespace MODEL.ResponseModels;

public class PushLogModel
{
    public string Title { get; set; }
    public string Body { get; set; }
    public int PushDateUnixTime { get; set; }
    public int PushTime { get; set; }
}