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

namespace GalogWeb.Controllers.APIControllers;

[Route("api/[controller]/[action]")]
public class QueryController : QarApiBaseController
{
    #region Properties
    private readonly IMemoryCache _memoryCache;
    public QueryController(IMemoryCache memoryCache) :base(memoryCache)
    {
        _memoryCache = memoryCache;
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
}