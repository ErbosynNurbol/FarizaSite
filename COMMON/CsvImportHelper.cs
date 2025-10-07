using Microsoft.AspNetCore.Http;

namespace COMMON;

using CsvHelper;
using System.Globalization;
using MODEL;

public static class CsvParseHelper
{
    public static List<ConsigneeData> ParseCsvFile(IFormFile csvFile)
    {
        var records = new List<ConsigneeData>();
        
        using (var reader = new StreamReader(csvFile.OpenReadStream()))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Read();
            csv.ReadHeader();
            
            while (csv.Read())
            {
                records.Add(new ConsigneeData
                {
                    Phone = csv.GetField("phone"),
                    Products = csv.GetField("products"),
                    Address = csv.GetField("address")
                });
            }
        }
        
        return records;
    }
}

public class ConsigneeData
{
    public string Phone { get; set; }
    public string Products { get; set; }
    public string Address { get; set; }
}