namespace MODEL.ResponseModels;

public class PersonPackageModel
{
    public string OrderNumber { get; set; }
    public string AddTime { get; set; }
    public List<string> ThumbnailList { get; set; }
    public PackagePointModel Sender { get; set; }
    public PackagePointModel Receiver { get; set; }
    public CourierModel Courier { get; set; }
    public string PackageInfo { get; set; }
    public byte Status { get; set; }
    public int Price { get; set; }
    public string ScheduleTime { get; set; }
    public double Distance { get; set; }
    public CourierPersonModel Person { get; set; }
    public class CourierPersonModel
    {
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }
    public class PackagePointModel
    {
        public string Location { get; set; }
        public string Time { get; set; }
        public string Entrance { get; set; }
        public string EntranceInfo { get; set; }
    }
    public class CourierModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
    }
}