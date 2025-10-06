namespace MODEL;

public partial class Region
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public string RegionNumber { get; set; }
    public string Map2gis { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int AddTime { get; set; }
    public int UpdateTime { get; set; }
    public byte QStatus { get; set; }
}