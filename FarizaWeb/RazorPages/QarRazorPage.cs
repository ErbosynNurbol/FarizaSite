using FarizaWeb.Caches;
using COMMON;
using Microsoft.AspNetCore.Mvc.Razor;
using MODEL;

namespace FarizaWeb.RazorPages;

public abstract class QarRazorPage<TModel> : RazorPage<TModel>
{
    private IWebHostEnvironment _environment;

    protected string T(string localKey)
    {
        if (string.IsNullOrWhiteSpace(localKey)) return localKey;
        var memoryCache = ViewContext.HttpContext.RequestServices.GetService<IMemoryCache>();
        return QarCache.GetLanguageValue(memoryCache, localKey, CurrentLanguage);
    }

    protected List<T> QarList<T>(string vdName) where T : new()
    {
        if (ViewData[vdName] is List<T> value)
        {
            return value;
        }

        return new List<T>();
    }

    protected T QarModel<T>(string vdName)
    {
        if (ViewData[vdName] is T value)
        {
            return value;
        }

        return default;
    }
    
    protected string GetUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        _environment ??= ViewContext.HttpContext.RequestServices.GetService<IWebHostEnvironment>();

        if (_environment == null || _environment.IsDevelopment())
        {
            return url.StartsWith("http") ? url : SiteUrl + url;
        }

        return url;
    }

    protected Additionalcontent GetAC(string additionalType)
    {
        var additionalContentList = QarList<Additionalcontent>("additionalContentList");
        return additionalContentList.FirstOrDefault(x =>
            x.AdditionalType.Equals(additionalType, StringComparison.OrdinalIgnoreCase));
    }
    protected string CurrentLanguage => (ViewData["language"] ?? string.Empty) as string;
    protected string CurrentTheme => QarSingleton.GetInstance().GetSiteTheme();
    protected string SiteUrl => QarSingleton.GetInstance().GetSiteUrl();
    protected string Query => (ViewData["query"] ?? string.Empty) as string;
    protected string ControllerName => (ViewData["controllerName"] ?? string.Empty) as string;
    protected string ActionName => (ViewData["actionName"] ?? string.Empty) as string;
    protected string SkinName => (ViewData["skinName"] ?? string.Empty) as string;
    protected string Title => (ViewData["title"] ?? string.Empty) as string;
    public List<Admin> UserList => (ViewData["userList"] ?? new  List<Admin>()) as  List<Admin>;
    protected List<Language> LanguageList => (ViewData["languageList"] ?? new List<Language>()) as List<Language>;
    protected List<Multilanguage> MultiLanguageList =>
        (ViewData["multiLanguageList"] ?? new List<Multilanguage>()) as List<Multilanguage>;
    protected Sitesetting SiteSetting => ViewData["siteSetting"] as Sitesetting;
    protected bool CanView => Convert.ToBoolean(ViewData["canView"] ?? false);
    protected bool CanCreate => Convert.ToBoolean(ViewData["canCreate"] ?? false);
    protected bool CanEdit => Convert.ToBoolean(ViewData["canEdit"] ?? false);
    protected bool CanDelete => Convert.ToBoolean(ViewData["canDelete"] ?? false);
 
}