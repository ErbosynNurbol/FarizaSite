namespace MODEL.RequestModels;

public class RequestPackageModel
{
    public string PackageInfo { get; set; }
    public string ScheduleTime { get; set; }
    public int Price { get; set; }
    public List<string> ImageList { get; set; }
    public List<string> MethodList { get; set; }
    public Option OptionList { get; set; }
    public PackagePointModel Sender { get; set; }
    public PackagePointModel Receiver { get; set; }

    public class Option
    {
        public bool MoreThan10Items { get; set; }
        public bool Require2People { get; set; }
    }

    public class PackagePointModel
    {
        public string Phone { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Location { get; set; }
        public string Entrance { get; set; }
        public string EntranceInfo { get; set; }
    }
}