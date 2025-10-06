using System.Globalization;
using FarizaWeb.Caches;
using COMMON;
using COMMON.Extensions;
using Dapper;
using DBHelper;
using Microsoft.AspNetCore.Authorization;
using MODEL;
using MODEL.FormatModels;
using MODEL.ViewModels;
using Serilog;
using System.Data;
using MODEL.Enums;
using MODEL.SurveyModal;

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
    
}