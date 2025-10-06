namespace MODEL.ResponseModels;

public class HistoryPackageModel
{
    public string OrderNumber { get; set; }
    public string AddTime { get; set; }
    public string PackageInfo { get; set; }
    public int Price { get; set; }
    public PackagePointModel Sender { get; set; }
    public PackagePointModel Receiver { get; set; }
    public PersonModel Person { get; set; }

    public class PackagePointModel
    {
        public string Location { get; set; }
        public string Time { get; set; }
    }

    public class PersonModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
    }
}