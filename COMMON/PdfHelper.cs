
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace COMMON;

// public class PdfHelper
// {
//     public static void ConvertPdfPageToImage(string pdfFilePath, string imageOutputPath)
//     {
//         new License().SetLicense("./libs/Aspose.Total.NET.lic");
//         var pdfDocument = new Document(pdfFilePath);

//         var page = pdfDocument.Pages[1];
//         var resolution = new Resolution(150);
//         var pngDevice = new PngDevice(resolution);
//         FileHelper.EnsureDir(imageOutputPath);
//         using (var imageStream = new MemoryStream())
//         {
//             pngDevice.Process(page, imageStream);
//             imageStream.Position = 0;

//             using (var skImage = SKImage.FromEncodedData(imageStream))
//             {
//                 using (var data = skImage.Encode(SKEncodedImageFormat.Png, 100))
//                 {
//                     using (var fileStream = File.OpenWrite(imageOutputPath))
//                     {
//                         data.SaveTo(fileStream);
//                     }
//                 }
//             }
//         }
//     }
// }


public class ReceiptParser
{
    public class ReceiptInfo
    {
        public string QRNumber { get; set; }
        public decimal Amount { get; set; }
    }
    
    public static ReceiptInfo ExtractReceiptInfo(string pdfPath)
    {
        try
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pdfPath.TrimStart('/'));
            
            using var reader = new PdfReader(fullPath);
            using var pdfDoc = new PdfDocument(reader);
            
            // 提取第一页文本
            var page = pdfDoc.GetPage(1);
            string text = PdfTextExtractor.GetTextFromPage(page);
            
            var result = new ReceiptInfo();
            
            // 提取QR号码（去掉QR前缀）
            var qrMatch = Regex.Match(text, @"QR(\d+)");
            if (qrMatch.Success)
            {
                result.QRNumber = qrMatch.Groups[1].Value; // 只返回数字部分
            }
            
            // 提取金额（查找数字+₸的模式）
            var amountMatch = Regex.Match(text, @"(\d+(?:\.\d+)?)\s*₸");
            if (amountMatch.Success)
            {
                if (decimal.TryParse(amountMatch.Groups[1].Value, out decimal amount))
                {
                    result.Amount = amount;
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"提取失败: {ex.Message}");
            return null;
        }
    }
}
