namespace MODEL.ResponseModels;

public class CourierPackageModel
{
    public string OrderNumber { get; set; }
    public string AddTime { get; set; }
    public int Price { get; set; }
    public byte Status { get; set; }
    public double Distance { get; set; }

    public string PackageInfo { get; set; }

    public PersonModel Person { get; set; }
    public PackagePointModel Sender { get; set; }

    public PackagePointModel Receiver { get; set; }

    public class PersonModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class PackagePointModel
    {
        public string Phone { get; set; }
        public string Location { get; set; }
        public string Entrance { get; set; }
        public string EntranceInfo { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}