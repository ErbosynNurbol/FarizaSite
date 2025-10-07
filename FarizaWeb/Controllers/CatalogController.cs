using COMMON;
using Dapper;
using DBHelper;
using FarizaWeb.Caches;
using Microsoft.AspNetCore.Authorization;
using MODEL;
using MODEL.FormatModels;
using Serilog;
using MODEL.Enums;

namespace FarizaWeb.Controllers;

[Authorize(Roles = "Admin")]
public class CatalogController : QarBaseController
{

    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _memoryCache;

    public CatalogController(IMemoryCache memoryCache, IWebHostEnvironment environment) : base(memoryCache, environment)
    {
        _memoryCache = memoryCache;
        _environment = environment;
    }

    #region About +About(string query)

    public IActionResult About(string query)
    {
        using var connection = Utilities.GetOpenConnection();
        ViewData["additionalContent"] = AdditionalContent(connection, ActionName);
        ViewData["title"] = T("ls_Aboutus");
        return View("~/Views/Console/AdditionalContent.cshtml");
    }

    #endregion

    #region Agreement +Agreement(string query)

    public IActionResult Agreement(string query)
    {
        using var connection = Utilities.GetOpenConnection();
        ViewData["additionalContent"] = AdditionalContent(connection, ActionName);
        ViewData["title"] = T("ls_Useragreement");
        return View("~/Views/Console/AdditionalContent.cshtml");
    }

    #endregion
    
    #region Client + Client(string query)

    public IActionResult Client(string query)
    {
        query = (query ?? string.Empty).Trim().ToLower();
        ViewData["query"] = query;
        ViewData["title"] = T("ls_Client");
        switch (query)
        {
            case "list":
                {
                    return View($"~/Views/Console/{ControllerName}/{ActionName}/List.cshtml");
                }
            default:
                {
                    return Redirect($"/{CurrentLanguage}/{ControllerName.ToLower()}/{ActionName.ToLower()}/list");
                }
        }
    }

    #endregion
    
    #region Get Client list +GetClientList(APIUnifiedModel model)

    [HttpPost]
    public IActionResult GetClientList(ApiUnifiedModel model)
    {
        var start = model.Start > 0 ? model.Start : 0;
        var length = model.Length > 0 ? model.Length : 10;
        var keyword = (model.Keyword ?? string.Empty).Trim();
        using var connection = Utilities.GetOpenConnection();
        var querySql = " from client where qStatus = 0 ";
        object queryObj = new { keyword = "%" + keyword + "%" };
        var orderSql = "";
        if (!string.IsNullOrEmpty(keyword)) querySql += " and (name like @keyword)";
        if (model.OrderList != null && model.OrderList.Count > 0)
            foreach (var item in model.OrderList)
                switch (item.Column)
                {
                    case 5:
                        {
                            orderSql += (string.IsNullOrEmpty(orderSql) ? "" : ",") + " addTime " + item.Dir;
                        }
                        break;
                }

        if (string.IsNullOrEmpty(orderSql)) orderSql = " addTime desc ";

        var total = connection.Query<int>("select count(1) " + querySql, queryObj).FirstOrDefault();
        var totalPage = total % length == 0 ? total / length : total / length + 1;
        var clientList = connection
            .Query<Client>("select * " + querySql + " order by " + orderSql + $" limit {start} , {length}", queryObj)
            .ToList();
        var regionList = QarCache.GetRegionList(_memoryCache);
        var dataList = clientList.Select(client => new
        {
             client.Id,
             client.Name,
             client.Phone,
             client.Address,
            client.Longitude,
            client.Latitude,
            client.ReceiptPath,
            avatarUrl = "/images/default_avatar.png",
            type = T($"{BillTypeHelper.GetStatusText(client.BillType)}"),  
            reginName = regionList.FirstOrDefault(r => r.Id == client.RegionId)?.RegionNumber ?? "",
            AddTime = UnixTimeHelper.UnixTimeToDateTime(client.AddTime).ToString("dd/MM/yyyy HH:mm")
        }).ToList();
        return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "",
            new { start, length, keyword, total, totalPage, dataList });
    }

    #endregion

    #region Set Client status +SetClientStatus(string manageType,List<int> idList)

    [HttpPost]
    public IActionResult SetClientStatus(string manageType, List<int> idList)
    {
        manageType = (manageType ?? string.Empty).Trim().ToLower();
        if (idList == null || idList.Count == 0)
            return MessageHelper.RedirectAjax(T("ls_Calo"), Status.Error, "", null);
        var currentTime = UnixTimeHelper.GetCurrentUnixTime();
        switch (manageType)
        {
            case "delete":
                {
                    using var connection = Utilities.GetOpenConnection();
                    using var tran = connection.BeginTransaction();
                    try
                    {
                        var clientList = connection
                            .GetList<Client>($"where qStatus = 0 and id in ({string.Join(",", idList)})").ToList();
                        foreach (var client in clientList)
                        {
                            client.QStatus = 1;
                            client.UpdateTime = currentTime;
                            connection.Update(Client);
                        }

                        tran.Commit();
                        return MessageHelper.RedirectAjax(T("ls_Deletedsuccessfully"), Status.Success, "", "");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, ActionName);
                        tran.Rollback();
                        return MessageHelper.RedirectAjax(T("ls_Savefailed"), Status.Error, "", "");
                    }
                }
            default:
                {
                    return MessageHelper.RedirectAjax(T("ls_Managetypeerror"), Status.Error, "", null);
                }
        }
    }

    #endregion
    
    #region Regions +Region(string query)

    public IActionResult Region(string query)
    {
        query = (query ?? string.Empty).Trim().ToLower();
        ViewData["query"] = query;
        ViewData["title"] = T("ls_Regions");
        using var connection = Utilities.GetOpenConnection();
        switch (query)
        {
            case "create":
                {
                    ViewData["cityList"] = QarCache.GetCityList(_memoryCache);
                    return View($"~/Views/Console/{ControllerName}/{ActionName}/CreateOrEdit.cshtml");
                }
            case "edit":
                {
                    var courierId = GetIntQueryParam("id", 0);
                    if (courierId <= 0)
                        return Redirect($"/{CurrentLanguage}/{ControllerName.ToLower()}/{ActionName.ToLower()}/list");
                    var region = connection
                        .GetList<Region>("where qStatus = 0 and id = @courierId ", new { courierId })
                        .FirstOrDefault();
                    if (region != null)
                        ViewData["region"] = region;
                    ViewData["cityList"] = QarCache.GetCityList(_memoryCache);
                    return View($"~/Views/Console/{ControllerName}/{ActionName}/CreateOrEdit.cshtml");
                }
            case "list":
                {
                    ViewData["regionList"] = QarCache.GetRegionList(_memoryCache);
                    return View($"~/Views/Console/{ControllerName}/{ActionName}/List.cshtml");
                }
        }
        return Redirect($"/{CurrentLanguage}/{ControllerName.ToLower()}/{ActionName.ToLower()}/list");
    }
    
    #endregion

    #region Region +Region(Admin item)

    [HttpPost]
    public IActionResult Region(Region item)
    {
        if (string.IsNullOrEmpty(item.RegionNumber))
            return MessageHelper.RedirectAjax(T("ls_Tfir"), Status.Error, "", $"regionNumber");

        if (string.IsNullOrEmpty(item.Map2gis))
            return MessageHelper.RedirectAjax(T("ls_Tfir"), Status.Error, "", $"northeast");

        if (CoordinateExtractor.TryExtractCoordinates(item.Map2gis, out double longitude, out double latitude))
        {
            item.Longitude = longitude;
            item.Latitude = latitude;
        }
        else
        {
            return MessageHelper.RedirectAjax(T("ls_Iuoutec"), Status.Error, "", $"northeast");
        }

        var currentTime = UnixTimeHelper.ConvertToUnixTime(DateTime.Now);

        using (var connection = Utilities.GetOpenConnection())
        {
            if (item.Id == 0)
                try
                {
                    if (connection.RecordCount<Region>("where qStatus = 0 and regionNumber = @regionNumber ",
                        new { regionNumber = item.RegionNumber }) > 0)
                        return MessageHelper.RedirectAjax(T("ls_Namealreadyexists"), Status.Error, "", "regionNumber");
                    item.AddTime = currentTime;
                    item.UpdateTime = currentTime;
                    item.QStatus = 0;
                    connection.Insert(item);
                    QarCache.ClearCache(_memoryCache, nameof(QarCache.GetRegionList));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ActionName);
                    return MessageHelper.RedirectAjax(T("ls_Savefailed"), Status.Error, "", "");
                }
            else
                try
                {
                    var region = connection.GetList<MODEL.Region>("where qStatus = 0 and id = @id", new { id = item.Id })
                        .FirstOrDefault();
                    if (region == null)
                        return MessageHelper.RedirectAjax(T("ls_Idoiiw"), Status.Error, "", "");

                    if (connection.RecordCount<Region>("where qStatus = 0 and regionNumber = @regionNumber and id <> id ",
                     new { regionNumber = item.RegionNumber, id = region.Id }) > 0)
                        return MessageHelper.RedirectAjax(T("ls_Namealreadyexists"), Status.Error, "", "regionNumber");

                    region.RegionNumber = item.RegionNumber;
                    region.CityId = item.CityId;
                    region.Map2gis = item.Map2gis;
                    region.Longitude = item.Longitude;
                    region.Latitude = item.Latitude;
                    region.UpdateTime = currentTime;
                    connection.Update(region);
                    QarCache.ClearCache(_memoryCache, nameof(QarCache.GetRegionList));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ActionName);
                    return MessageHelper.RedirectAjax(T("ls_Savefailed"), Status.Error, "", "");
                }
        }

        return MessageHelper.RedirectAjax(T("ls_Addedsuccessfully"), Status.Success,
            $"/{CurrentLanguage}/{ControllerName}/{ActionName}/list", "");
    }

    #endregion

    #region Get region list +GetRegionList(APIUnifiedModel model)
    [HttpPost]
    public IActionResult GetRegionList(ApiUnifiedModel model)
    {
        var start = model.Start > 0 ? model.Start : 0;
        var length = model.Length > 0 ? model.Length : 10;
        var keyWord = (model.Keyword ?? string.Empty).Trim();
        using var connection = Utilities.GetOpenConnection();
        var querySql = " from region where qStatus = 0 ";
        object queryObj = new { keyWord = "%" + keyWord + "%" };
        var orderSql = "";
        if (!string.IsNullOrEmpty(keyWord)) querySql += " and (regionNumber like @keyWord)";
        if (model.OrderList is { Count: > 0 })
            foreach (var item in model.OrderList)
                switch (item.Column)
                {
                    case 3:
                        {
                            orderSql += (string.IsNullOrEmpty(orderSql) ? "" : ",") + " addTime " + item.Dir;
                        }
                        break;
                }

        if (string.IsNullOrEmpty(orderSql))
            orderSql = " addTime desc ";

        var total = connection.Query<int>("select count(1) " + querySql, queryObj).FirstOrDefault();
        var totalPage = total % length == 0 ? total / length : total / length + 1;
        var sql = "select * " + querySql + " order by " + orderSql + $" limit {start} , {length}";
        var regionList = connection.Query<MODEL.Region>(sql, queryObj).ToList();
        var cityList = QarCache.GetCityList(_memoryCache);
        var dataList = regionList.Select(x => new
        {
            x.Id,
            x.RegionNumber,
            cityList.FirstOrDefault(c => c.Id == x.CityId)?.CityName,
            x.Map2gis,
            x.Longitude,
            x.Latitude,
            AddDate = UnixTimeHelper.UnixTimeToDateTime(x.AddTime).ToString("MM/dd/yyyy"),
            AddTime = UnixTimeHelper.UnixTimeToDateTime(x.AddTime).ToString("HH:mm:ss")
        });
        return MessageHelper.RedirectAjax(T("ls_Searchsuccessful"), Status.Success, "",
            new { start, length, keyWord, total, totalPage, dataList });
    }

    #endregion

    #region Set region status +SetRegionStatus(string manageType,List<int> idList)
    [HttpPost]
    public IActionResult SetRegionStatus(string manageType, List<int> idList)
    {
        manageType = (manageType ?? string.Empty).Trim().ToLower();
        if (idList == null || idList.Count == 0)
            return MessageHelper.RedirectAjax(T("ls_Chooseatleastone"), Status.Error, "", null);
        var currentTime = UnixTimeHelper.ConvertToUnixTime(DateTime.Now);
        switch (manageType)
        {
            case "delete":
                {
                    using var connection = Utilities.GetOpenConnection();
                    using var tran = connection.BeginTransaction();
                    try
                    {
                        var regionList = connection
                            .GetList<MODEL.Region>($"where qStatus = 0 and id in ({string.Join(",", idList)})")
                            .ToList();
                        foreach (var region in regionList)
                        {
                            region.QStatus = 1;
                            region.UpdateTime = currentTime;
                            connection.Update(region);
                        }

                        tran.Commit();
                        QarCache.ClearCache(_memoryCache, nameof(QarCache.GetRegionList));
                        return MessageHelper.RedirectAjax(T("ls_Deletedsuccessfully"), Status.Success, "", "");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, ActionName);
                        tran.Rollback();
                        return MessageHelper.RedirectAjax(T("ls_Savefailed"), Status.Error, "", "");
                    }
                }
            default:
                {
                    return MessageHelper.RedirectAjax(T("ls_Managetypeerror"), Status.Error, "", null);
                }
        }
    }

    #endregion
    
    #region Жарнама +Advertise(APIUnifiedModel model)
    public IActionResult Advertise(string query)
    {
        query = (query ?? string.Empty).Trim().ToLower();
        ViewData["query"] = query;
        ViewData["title"] = T("ls_Regions");
        using var connection = Utilities.GetOpenConnection();
        switch (query)
        {
            case "list":
            {
                return View($"~/Views/Console/{ControllerName}/{ActionName}/List.cshtml");
            }
        }
        return Redirect($"/{CurrentLanguage}/{ControllerName.ToLower()}/{ActionName.ToLower()}/list");
    }
    
    [HttpPost]
    public IActionResult Advertise()
    {
        using (var _connection = Utilities.GetOpenConnection())
        {
            Consignee person = _connection.Query<Consignee>(
                "SELECT id, phone FROM consignee WHERE qStatus = 0 AND isSendSms = 0 AND id >= (SELECT FLOOR(RAND() * (SELECT MAX(id) FROM consignee))) ORDER BY id LIMIT 1"
            ).FirstOrDefault();
        
            if (person != null)
            {
                // person.Phone = "+77003142857";
                string msgText = "Сәлеметсіз бе!Сіздің Алмасбек Батырдан алған тауарыңыз дайын. Оны алып кетуге немесе үйге дейін жеткізуге тапсырыс беруге болады.Төмендегі сілтеме арқылы қалауыңызды таңдаңыз {url}?phone={phone}";
                msgText = msgText.Replace("{url}", "https://almasbek-batyr.3100.kz");
                msgText = msgText.Replace("{phone}", person.Phone);
                string encodedMessage = System.Net.WebUtility.UrlEncode(msgText);
                string whatsappUrl = $"https://wa.me/{person.Phone.Replace("+","")}?text={encodedMessage}";
                _connection.Execute(
                    "UPDATE consignee SET isSendSms = 1 WHERE id = @personId", 
                    new { personId = person.Id }
                );
            
                return MessageHelper.RedirectAjax(T("ls_Flushsuccessfully"), Status.Success, whatsappUrl, null);
            }
            return MessageHelper.RedirectAjax(T("ls_Complete"), Status.Success, "", null);
        }
    }
    #endregion

    #region Kaspibill

    public IActionResult Kaspibill(string query)
    {
        query = (query ?? string.Empty).Trim().ToLower();
        ViewData["query"] = query;
        ViewData["title"] = T("ls_Bill");
        using var connection = Utilities.GetOpenConnection();
        switch (query)
        {
            case "list":
            {
                return View($"~/Views/Console/{ControllerName}/{ActionName}/List.cshtml");
            }
        }
        return Redirect($"/{CurrentLanguage}/{ControllerName.ToLower()}/{ActionName.ToLower()}/list");
    }
    

    #endregion
}