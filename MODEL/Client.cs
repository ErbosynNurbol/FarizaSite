namespace MODEL;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Map2Gis { get; set; }
    public string ReceiptPath { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RegionId { get; set; }
    public int AddTime { get; set; }
    public int UpdateTime { get; set; }
    public byte QStatus { get; set; }
}