namespace MODEL;

public class Consignee
{
    public int Id { get; set; }
    public string Phone { get; set; }
    public int AddTime { get; set; }
    public byte QStatus { get; set; }
    public byte IsSendSms { get; set; }
    public string Products { get; set; }
    public string Address { get; set; }
}