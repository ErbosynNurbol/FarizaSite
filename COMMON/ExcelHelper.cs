
using Aspose.Cells;

namespace COMMON;

public class ExcelHelper
{
    public class KaspiPaymentData
    {
        public string OperationAmount { get; set; }  // Операция сомасы
        public string OperationNumber { get; set; }  // Операция нөмірі
    }

    public static List<KaspiPaymentData> ReadKaspiPayments(string filePath)
    {
        var payments = new List<KaspiPaymentData>();
        var workbook = new Workbook(filePath);
        var worksheet = workbook.Worksheets[0];
        var cells = worksheet.Cells;
        var maxDataRow = cells.MaxDataRow;
    
        int amountCol = -1, numberCol = -1, startRow = 0;
    
        // 查找列标题
        for (var row = 0; row <= Math.Min(20, maxDataRow); row++)
        {
            for (var col = 0; col < 20; col++)
            {
                var value = cells[row, col].Value?.ToString()?.ToLower()?.Trim();
                if (string.IsNullOrEmpty(value)) continue;
            
                if (value.Contains("сомасы") || value.Contains("сумма"))
                    amountCol = col;
                if (value.Contains("нөмірі") || value.Contains("номер"))
                    numberCol = col;
            }
        
            if (amountCol >= 0 && numberCol >= 0)
            {
                startRow = row + 1;
                break;
            }
        }
    
        // 读取数据
        if (amountCol >= 0 && numberCol >= 0)
        {
            for (var row = startRow; row <= maxDataRow; row++)
            {
                try
                {
                    var amount = cells[row, amountCol].Value?.ToString()?.Trim();
                    var number = cells[row, numberCol].Value?.ToString()?.Trim();
                
                    if (!string.IsNullOrEmpty(amount) || !string.IsNullOrEmpty(number))
                    {
                        payments.Add(new KaspiPaymentData
                        {
                            OperationAmount = amount,
                            OperationNumber = number
                        });
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    
        return payments;
    }
}