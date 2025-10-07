namespace MODEL;

public class Client
{
    public int Id { get; set; }
    public int ConsigneeId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string ReceiptPath { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RegionId { get; set; }
    public string ShipperPhone { get; set; }
    public int AddTime { get; set; }
    public int UpdateTime { get; set; }
    public byte QStatus { get; set; }
    public byte BillType { get; set; }
    public string BillNumber { get; set; }
    public int BillAmount { get; set; }
    
}