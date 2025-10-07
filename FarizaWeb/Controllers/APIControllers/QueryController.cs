using System.Globalization;
using System.Text;
using COMMON;
using Dapper;
using DBHelper;
using Microsoft.AspNetCore.Authorization;
using MODEL;
using MODEL.Enums;
using MODEL.RequestModels;
using MODEL.ViewModels;
using Serilog;
using FarizaWeb.Caches;
using FarizaWeb.Controllers;
using System.Globalization;
using MODEL.FormatModels;

namespace GalogWeb.Controllers.APIControllers;

[Route("api/[controller]/[action]")]
public class QueryController : QarApiBaseController
{
    #region Properties

    private readonly IMemoryCache _memoryCache;
    private readonly IWebHostEnvironment _environment;

    public QueryController(IMemoryCache memoryCache,IWebHostEnvironment environment) : base(memoryCache,environment)
    {
        _memoryCache = memoryCache;
        _environment = environment;
    }

    public ReceiptParser.ReceiptInfo GetReceiptInfo(string receiptPath)
    {
        string fullPath = Path.Combine(_environment.WebRootPath, receiptPath.TrimStart('/'));
        return ReceiptParser.ExtractReceiptInfo(fullPath);
    }
    #endregion

    #region Language +Language(string query)

    [AllowAnonymous]
    [HttpGet("{query}")]
    public IActionResult Language(string query)
    {
        query = (query ?? string.Empty).Trim().ToLower();
        switch (query)
        {
            case "default":
            {
                var allLanguageList = QarCache.GetLanguageList(_memoryCache);

                var defaultLanguage = allLanguageList.FirstOrDefault(x => x.IsDefault == 1 && x.FrontendDisplay == 1) ??
                                      allLanguageList.FirstOrDefault(x => x.IsDefault == 1);

                return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "",
                    defaultLanguage?.Culture ?? string.Empty);
            }
            case "list":
            {
                var allLanguageList = QarCache.GetLanguageList(_memoryCache);
                var languageList = allLanguageList.Where(x => x.FrontendDisplay == 1).ToList();
                if (!languageList.Any())
                {
                    var defaultLanguage = allLanguageList.FirstOrDefault(x => x.IsDefault == 1);
                    if (defaultLanguage != null)
                    {
                        languageList.Add(defaultLanguage);
                    }
                }

                return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "", languageList.Select(x =>
                    new
                    {
                        x.FullName,
                        x.ShortName,
                        x.Culture,
                        x.UniqueSeoCode,
                        IsDefault = x.IsDefault == 1,
                        FlagUrl = UrlHelper.GetFullUrl(x.FlagUrl)
                    }));
            }
            default:
                return NotFound();
        }
    }

    #endregion

    #region About +About()

    [HttpGet]
    public IActionResult About()
    {
        var additionalContentList = QarCache.GetAdditionalContentList(_memoryCache, CurrentLanguage);
        var about = additionalContentList.FirstOrDefault(x =>
            x.AdditionalType.Equals("About", StringComparison.OrdinalIgnoreCase)) ?? new Additionalcontent();
        return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "", about.FullDescription);
    }

    #endregion

    #region Agreement +Agreement()

    [HttpGet]
    public IActionResult Agreement()
    {
        var additionalContentList = QarCache.GetAdditionalContentList(_memoryCache, CurrentLanguage);
        var agreement = additionalContentList.FirstOrDefault(x =>
            x.AdditionalType.Equals("Agreement", StringComparison.OrdinalIgnoreCase)) ?? new Additionalcontent();
        return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "", agreement.FullDescription);
    }

    #endregion

    #region Geo data List +GeoDataList([FromBody] FilterModel model)

    [HttpPost]
    public IActionResult GeoDataList([FromBody] FilterModel model)
    {
        if (model == null)
            return MessageHelper.RedirectAjax(T("ls_Objectisempty"), Status.Error, "", "");

        model.PageSize = model.PageSize <= 0 ? 10 : model.PageSize;
        model.PageSize = model.PageSize >= 100 ? 10 : model.PageSize;

        var takeCount = model.PageSize;
        var offset = (model.PageOffset > 0 ? model.PageOffset : 0) * takeCount;

        try
        {
            using var connection = Utilities.GetOpenConnection();

            var sqlBuilder = new StringBuilder(@"
            SELECT SQL_CALC_FOUND_ROWS
                Id, kzTitle, kzRegion, kzDistrict, ruTitle, ruRegion, ruDistrict,
                enTitle, enRegion, enDistrict, Lat, Lng");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(model.SearchTerm))
            {
                sqlBuilder.Append(@",
                MATCH(kzTitle, kzRegion, kzDistrict, ruTitle, ruRegion, ruDistrict, enTitle, enRegion, enDistrict)
                AGAINST(@term IN BOOLEAN MODE) AS relevance
                FROM geodata
                WHERE qStatus = 0
                AND MATCH(kzTitle, kzRegion, kzDistrict, ruTitle, ruRegion, ruDistrict, enTitle, enRegion, enDistrict)
                AGAINST(@term IN BOOLEAN MODE)");

                parameters.Add("term", $"{model.SearchTerm}*");
                sqlBuilder.Append(" ORDER BY relevance DESC");
            }
            else
            {
                sqlBuilder.Append(@"
                FROM geodata
                WHERE qStatus = 0
                ORDER BY Id ASC");
            }

            sqlBuilder.Append(" LIMIT @offset, @takeCount;");

            parameters.Add("offset", offset);
            parameters.Add("takeCount", takeCount);

            var geoDataList = connection.Query(sqlBuilder.ToString(), parameters).ToList();

            var totalCount = connection.ExecuteScalar<int>("SELECT FOUND_ROWS();");

            var result = geoDataList.Select(x =>
            {
                string title, region, district;
                switch (CurrentLanguage.ToLower())
                {
                    case "ru":
                        title = x.ruTitle;
                        region = x.ruRegion;
                        district = x.ruDistrict;
                        break;
                    case "en":
                        title = x.enTitle;
                        region = x.enRegion;
                        district = x.enDistrict;
                        break;
                    default: // kz or fallback
                        title = x.kzTitle;
                        region = x.kzRegion;
                        district = x.kzDistrict;
                        break;
                }

                return new
                {
                    Id = x.Id,
                    Title = title,
                    Region = region,
                    District = district,
                    Lat = x.Lat,
                    Lng = x.Lng
                };
            }).ToList();

            return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "", new
            {
                DataList = result,
                TotalCount = totalCount,
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GeodataList");
            return MessageHelper.RedirectAjax(T("ls_Oswwptal"), Status.Error, "", "");
        }
    }

    #endregion


    #region Client
    [HttpPost]
    public IActionResult Client([FromForm] Client client,[FromForm] IFormFile receipt)
    {
        
        if (string.IsNullOrEmpty(client.Name))
            return MessageHelper.RedirectAjax(T("ls_Tfir"), Status.Error, "", "Name");
        if (string.IsNullOrEmpty(client.Phone) || !RegexHelper.IsPhoneNumber(client.Phone, out string phoneNumber))
            return MessageHelper.RedirectAjax(T("ls_Tfir"), Status.Error, "", "Phone");
        client.Phone = phoneNumber;
        if (string.IsNullOrEmpty(client.Address))
            return MessageHelper.RedirectAjax(T("ls_Tfir"), Status.Error, "", "Address");
        if (receipt == null)
            return MessageHelper.RedirectAjax(T("ls_Psaftu"), Status.Error, "", "receipt");
        
        var receiptFileUrl = SaveToFile(receipt);
        var receiptInfo = ReceiptParser.ExtractReceiptInfo(receiptFileUrl);
        string billNumber = "";
        int billAmount = 0;
        if (receiptInfo != null)
        {
            billNumber = receiptInfo.QRNumber ?? "";
            billAmount = Convert.ToInt32(receiptInfo.Amount);
        }
        else
        {
            return MessageHelper.RedirectAjax("Чек жарамсыз", Status.Error, "", "BillNumber");
        }
                
        using var connection = Utilities.GetOpenConnection();
        var currentTime = UnixTimeHelper.GetCurrentUnixTime();
        try
        {
            int? res;
            // 1. 先检查consignee
            var consignee = connection.GetList<Consignee>("WHERE qStatus = 0 AND phone = @phone", new { client.Phone }).FirstOrDefault();
            if (consignee == null)
            {
                return MessageHelper.RedirectAjax("no peple", Status.Error, "", "Phone");
            }

            var existingBill = connection.GetList<Client>("WHERE qStatus = 0 AND billNumber = @billNumber", new { billNumber }).FirstOrDefault();
            if (existingBill != null)
            {
                return MessageHelper.RedirectAjax("Чек жарамсыз", Status.Error, "", "BillNumber");
            }
            
            res = connection.Insert(new Client
            {
                Name = client.Name,
                ConsigneeId = consignee.Id,
                BillNumber = billNumber,
                BillAmount = billAmount,
                Address = client.Address,
                Phone = client.Phone,
                Latitude = client.Latitude,
                Longitude = client.Longitude,
                ReceiptPath = receiptFileUrl,
                AddTime = currentTime,
                UpdateTime = currentTime,
                QStatus = 0
            });
            if (res > 0)
            {
                return MessageHelper.RedirectAjax(T("ls_Addedsuccessfully"), Status.Success,
                    "", "");
            }
        }
        catch (Exception e)
        {

            return MessageHelper.RedirectAjax(T("ls_Savefailed"), Status.Error, "", "");
        }
        return MessageHelper.RedirectAjax(T("ls_Savefailed"), Status.Error, "", "");

    }


    #endregion

    #region Get + Client
    [HttpPost]
    public IActionResult GetServiceList(ApiUnifiedModel model)
    {
        var start = model.Start > 0 ? model.Start : 0;
        var length = model.Length > 0 ? model.Length : 10;
        var keyWord = (model.Keyword ?? string.Empty).Trim();
        using var connection = Utilities.GetOpenConnection();
        var querySql = " from client where qStatus = 0 ";
        object queryObj = new { keyWord = "%" + keyWord + "%" };
        var orderSql = "";
        if (!string.IsNullOrEmpty(keyWord))
        {
            querySql += " and (phone like @keyWord)";
        }

        if (model.OrderList is { Count: > 0 })
        {
            foreach (var item in model.OrderList)
            {
                switch (item.Column)
                {
                    case 3:
                    {
                        orderSql += (string.IsNullOrEmpty(orderSql) ? "" : ",") + " addTime " + item.Dir;
                    }
                        break;
                }
            }
        }

        if (string.IsNullOrEmpty(orderSql))
        {
            orderSql = " addTime desc ";
        }

        var total = connection.Query<int>("select count(1) " + querySql, queryObj).FirstOrDefault();
        var totalPage = total % length == 0 ? total / length : total / length + 1;
        var serviceList = connection
            .Query<Client>("select * " + querySql + " order by " + orderSql + $" limit {start} , {length}",
                queryObj).ToList();

        var dataList = serviceList.Select(x => new
        {
            x.Id,
            x.Name,
            x.Phone,
            x.Address,
            x.ReceiptPath,
            AddTime = UnixTimeHelper.UnixTimeToDateTime(x.AddTime).ToString("dd/MM/yyyy HH:mm")
        }).ToList();
        return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "",
            new { start, length, keyWord, total, totalPage, dataList });
    }
    

    #endregion
    
    
    // [HttpPost]
    // public IActionResult ImportCsv([FromForm] IFormFile csvFile)
    // {
    //     try
    //     {
    //         var csvData = CsvParseHelper.ParseCsvFile(csvFile);
    //         var currentTime = UnixTimeHelper.GetCurrentUnixTime();
    //     
    //         using var connection = Utilities.GetOpenConnection();
    //         int successCount = 0;
    //     
    //         foreach (var data in csvData)
    //         {
    //             var result = connection.Insert(new Consignee
    //             {
    //                 Phone = data.Phone,
    //                 Products = data.Products,
    //                 Address = data.Address,
    //                 AddTime = currentTime,
    //                 QStatus = 0,
    //                 IsSendSms = 1
    //             });
    //         
    //             if (result > 0) successCount++;
    //         }
    //     
    //         return MessageHelper.RedirectAjax($"成功导入 {successCount} 条记录", Status.Success, "", "");
    //     }
    //     catch (Exception ex)
    //     {
    //         return MessageHelper.RedirectAjax($"导入失败: {ex.Message}", Status.Error, "", "");
    //     }
    // }
}