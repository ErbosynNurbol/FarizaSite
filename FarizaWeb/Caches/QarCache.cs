using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using FarizaWeb.Attributes;
using FarizaWeb.Controllers;
using COMMON;
using Dapper;
using DBHelper;
using Microsoft.AspNetCore.Mvc.Controllers;
using MODEL;
using MODEL.ViewModels;
using static COMMON.WeatherHelper;
using MODEL.SurveyModal;
using SkiaSharp;

namespace FarizaWeb.Caches;

public class QarCache
{
    private static string GetMethodName([CallerMemberName] string methodName = "")
    {
        return methodName;
    }

   
    #region Түрлер қатарын алу +GetCacheObject(IMemoryCache _memoryCache, string cacheName)

    public static object GetCacheObject(IMemoryCache memoryCache, string cacheName)
    {
        return !memoryCache.TryGetValue<object>(cacheName, out var cacheValue) ? null : cacheValue;
    }

    #endregion

    #region Түрлер қатарын Теңшеу +SetCacheObject(IMemoryCache _memoryCache, string cacheName, object cacheValue, int minute = 1)

    public static void SetCacheObject(IMemoryCache memoryCache, string cacheName, object cacheValue, int minute = 1)
    {
        if (cacheValue == null)
            memoryCache.Remove(cacheName);
        else
            memoryCache.Set(cacheName, cacheValue, TimeSpan.FromMinutes(minute));
    }

    #endregion

    #region Clear Cache +ClearCache(IMemoryCache _memoryCache, string cacheName)

    public static void ClearCache(IMemoryCache memoryCache, string cacheName, int id = 0)
    {
        memoryCache.Remove(cacheName);
        memoryCache.Remove($"{cacheName}_1");
        memoryCache.Remove($"{cacheName}_{id}");
        foreach (var language in GetLanguageList(memoryCache))
        {
            memoryCache.Remove($"{cacheName}_{language.Culture}");
            for (var i = 1; i <= 30; i++) memoryCache.Remove($"{cacheName}_{language.Culture}_{i}");
        }
    }

    #endregion

    #region Update Entity List With MultiLanguage +UpdateEntityListWithMultiLanguage<T>(IDbConnection _connection, List<T> entities, string language, List<string> keyList) where T : class

    private static void UpdateEntityListWithMultiLanguage<T>(IDbConnection connection, List<T> entityList,
        string language, List<string> keyList) where T : class
    {
        if (entityList == null || !entityList.Any())
            return;

        var idList = entityList.ConvertAll(e =>
        {
            var value = e.GetType().GetProperty("Id")?.GetValue(e);
            return value != null ? (int)value : 0;
        });

        var multiLanguageList = QarBaseController
            .GetMultilanguageList(connection, typeof(T).Name, idList, null, language)
            .ToList();

        foreach (var entity in entityList)
        foreach (var key in keyList)
        {
            var multiLanguageItem = multiLanguageList.FirstOrDefault(x =>
                x.ColumnId == (int)entity.GetType().GetProperty("Id")!.GetValue(entity)! &&
                x.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (multiLanguageItem == null || string.IsNullOrEmpty(multiLanguageItem.ColumnValue)) continue;
            var propertyInfo = entity.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null && propertyInfo.CanWrite)
                propertyInfo.SetValue(entity, multiLanguageItem.ColumnValue, null);
        }
    }

    #endregion

    #region Get Language List +GetLanguageList(IMemoryCache _memoryCache)

    public static List<Language> GetLanguageList(IMemoryCache memoryCache)
    {
        var cacheName = GetMethodName();
        if (memoryCache.TryGetValue(cacheName, out List<Language> list)) return list;
        using var connection = Utilities.GetOpenConnection();
        list = connection.GetList<Language>("where qStatus = 0")
            // .Select(x => new Language
            // {
            //     Id = x.Id,
            //     ShortName = x.ShortName,
            //     FullName = x.FullName,
            //     FlagUrl = x.FlagUrl,
            //     Culture = x.Culture,
            //     IsDefault = x.IsDefault,
            //     FrontendDisplay = x.FrontendDisplay,
            //     BackendDisplay = x.BackendDisplay,
            //     DisplayOrder = x.DisplayOrder
            // })
            .OrderBy(x => x.DisplayOrder).ToList();
        memoryCache.Set(cacheName, list, TimeSpan.FromDays(7));

        return list;
    }

    #endregion

    #region Get Language Pack Dictionary -GetLanguagePackDictionary(IMemoryCache memoryCache)

    public static Dictionary<string, Dictionary<string, string>> GetLanguagePackDictionary(IMemoryCache memoryCache)
    {
        var cacheName = GetMethodName();
        if (memoryCache.TryGetValue(cacheName, out Dictionary<string, Dictionary<string, string>> allLanguagePackDic))
            return allLanguagePackDic;
        allLanguagePackDic = new Dictionary<string, Dictionary<string, string>>();

        var languageList = GetLanguageList(memoryCache);
        string[] kzSubLanguages = { "tote", "latyn" };

        var jsonLanguagePack = LanguagePackHelper.GetLanguagePackJsonString();
        if (string.IsNullOrEmpty(jsonLanguagePack)) return allLanguagePackDic;

        var languagePackDictionary =
            JsonHelper.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonLanguagePack);

        foreach (var entry in languagePackDictionary)
        {
            if (allLanguagePackDic.ContainsKey(entry.Key)) continue;
            var currentLanguagePackDic = new Dictionary<string, string>();
            foreach (var valueEntry in entry.Value)
            {
                var key = valueEntry.Key.ToLower().Trim();

                var isKzSubLanguageExistsInLanguageList = key.Equals("kz", StringComparison.OrdinalIgnoreCase)
                                                          && languageList.Exists(
                                                              x => kzSubLanguages.Contains(x.Culture));

                if (isKzSubLanguageExistsInLanguageList)
                {
                    if (languageList.Exists(x =>
                            x.Culture.Equals("kz", StringComparison.OrdinalIgnoreCase)) &&
                        !currentLanguagePackDic.ContainsKey("kz"))
                        currentLanguagePackDic.TryAdd("kz", valueEntry.Value);

                    if (languageList.Exists(x =>
                            x.Culture.Equals("latyn", StringComparison.OrdinalIgnoreCase)) &&
                        !currentLanguagePackDic.ContainsKey("latyn"))
                        currentLanguagePackDic.TryAdd("latyn", Cyrl2LatynHelper.Cyrl2Latyn(valueEntry.Value));

                    if (languageList.Exists(x =>
                            x.Culture.Equals("tote", StringComparison.OrdinalIgnoreCase)) &&
                        !currentLanguagePackDic.ContainsKey("tote"))
                        currentLanguagePackDic.TryAdd("tote", Cyrl2ToteHelper.Cyrl2Tote(valueEntry.Value));
                    continue;
                }

                var isKeyExistsInLanguageList = languageList.Exists(x =>
                    x.Culture.Equals(key, StringComparison.OrdinalIgnoreCase));

                if (isKeyExistsInLanguageList)
                {
                    currentLanguagePackDic.TryAdd(key, valueEntry.Value);
                }
            }

            allLanguagePackDic.Add(entry.Key, currentLanguagePackDic);
        }

        memoryCache.Set(cacheName, allLanguagePackDic, TimeSpan.FromDays(1));
        return allLanguagePackDic;
    }

    #endregion

    #region Get Language Value +GetLanguageValue(IMemoryCache memoryCache, string localKey, string language)

    public static string GetLanguageValue(IMemoryCache memoryCache, string localKey, string language)
    {
        language = (language ?? string.Empty).ToLower().Trim();
        if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(localKey)) return localKey;

        var languagePackDictionary = GetLanguagePackDictionary(memoryCache);
        if (languagePackDictionary.TryGetValue(localKey, out var packDictionary))
        {
            if (packDictionary.TryGetValue(language, out var res)) return res;
            if (packDictionary.TryGetValue("en", out res)) return res;
        }

        return localKey;
    }

    #endregion
    
    #region Барлық мениюлерді алу +GetNavigationList(IMemoryCache _memoryCache, int navigationTypeId = 1)

    public static List<Navigation> GetNavigationList(IMemoryCache memoryCache, int navigationTypeId = 1)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}_{navigationTypeId}";
        if (!memoryCache.TryGetValue(cacheName, out List<Navigation> list))
        {
            using var connection = Utilities.GetOpenConnection();
            list = connection
                .GetList<Navigation>(
                    "where qStatus = 0 and navigationTypeId =  @navigationTypeId order by displayOrder asc ",
                    new { navigationTypeId }).ToList();
            memoryCache.Set(cacheName, list, DateTimeOffset.MaxValue);
        }

        return list;
    }

    #endregion

    #region Get Site Setting +GetSiteSetting(IMemoryCache _memoryCache)

    public static Sitesetting GetSiteSetting(IMemoryCache memoryCache)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}";
        if (!memoryCache.TryGetValue(cacheName, out Sitesetting siteSetting))
        {
            using var connection = Utilities.GetOpenConnection();
            siteSetting = connection.GetList<Sitesetting>("where qStatus = 0 ").FirstOrDefault();
            if (siteSetting == null)
            {
                siteSetting = new Sitesetting
                {
                    LightLogo = "",
                    DarkLogo = "",
                    MobileLightLogo = "",
                    MobileDarkLogo = "",
                    Title = "",
                    Description = "",
                    Keywords = "",
                    AnalyticsHtml = "",
                    Copyright = "",
                    AnalyticsScript = "",
                    Address = "",
                    Phone = "",
                    Email = "",
                    MapEmbed = "",
                    Facebook = "",
                    Twitter = "",
                    Instagram = "",
                    Vk = "",
                    SEmail = "",
                    Telegram = "",
                    Youtube = "",
                    Whatsapp = "",
                    Tiktok = "",
                    LoginPath = "",
                    MaxErrorCount = 10,
                    Favicon = "",
                    QStatus = 0
                };
                siteSetting.Id = connection.Insert(siteSetting) ?? 0;
            }

            memoryCache.Set(cacheName, siteSetting, DateTimeOffset.MaxValue);
        }

        return siteSetting;
    }

    #endregion

    #region Get Additional Content List +GetAdditionalContentList(IMemoryCache _memoryCache, string language)

    public static List<Additionalcontent> GetAdditionalContentList(IMemoryCache memoryCache, string language)
    {
        switch (language)
        {
            case "latyn":
            case "tote":
            {
                language = "kz";
            }
                break;
        }

        var cacheName = $"{MethodBase.GetCurrentMethod().Name}_{language}";
        if (!memoryCache.TryGetValue(cacheName, out List<Additionalcontent> list))
        {
            using var connection = Utilities.GetOpenConnection();
            list = connection.GetList<Additionalcontent>("where qStatus = 0 ").ToList();
            UpdateEntityListWithMultiLanguage(connection, list, language,
                new List<string>
                {
                    nameof(Additionalcontent.Title), nameof(Additionalcontent.ShortDescription),
                    nameof(Additionalcontent.FullDescription)
                });
            memoryCache.Set(cacheName, list, TimeSpan.FromHours(1));
        }

        return list;
    }

    #endregion

    #region Get Role List +GetRoleList(IMemoryCache _memoryCache, string language)

    public static List<Role> GetRoleList(IMemoryCache memoryCache, string language)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}_{language}";
        if (!memoryCache.TryGetValue(cacheName, out List<Role> list))
        {
            using var connection = Utilities.GetOpenConnection();
            list = connection.GetList<Role>("where qStatus = 0 ").ToList();
            UpdateEntityListWithMultiLanguage(connection, list, language,
                new List<string> { nameof(Role.Name), nameof(Role.Description) });
            memoryCache.Set(cacheName, list, TimeSpan.FromMinutes(10));
        }

        return list;
    }

    #endregion

    #region Get Role List +GetRoleList(IMemoryCache _memoryCache, string language)

    public static List<Adminrole> GetAdminroleList(IMemoryCache memoryCache)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}";
        if (!memoryCache.TryGetValue(cacheName, out List<Adminrole> list))
        {
            using var connection = Utilities.GetOpenConnection();
            list = connection.GetList<Adminrole>("where qStatus = 0 ").ToList();
            memoryCache.Set(cacheName, list, TimeSpan.FromMinutes(10));
        }

        return list;
    }

    #endregion

    #region Get Permission List +GetPermissionList(IMemoryCache _memoryCache)

    public static List<Permission> GetPermissionList(IMemoryCache memoryCache)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}";
        if (!memoryCache.TryGetValue(cacheName, out List<Permission> list))
        {
            using var connection = Utilities.GetOpenConnection();
            list = connection.GetList<Permission>("where qStatus = 0").ToList();
            memoryCache.Set(cacheName, list, DateTimeOffset.MaxValue);
        }

        return list;
    }

    #endregion

    #region Get Role Permission List +GetRolePermissionList(IMemoryCache _memoryCache)

    public static List<Rolepermission> GetRolePermissionList(IMemoryCache memoryCache)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}";
        if (!memoryCache.TryGetValue(cacheName, out List<Rolepermission> list))
        {
            using var connection = Utilities.GetOpenConnection();
            list = connection.GetList<Rolepermission>("where qStatus = 0").ToList();
            memoryCache.Set(cacheName, list, DateTimeOffset.MaxValue);
        }

        return list;
    }

    #endregion

    #region Get Navigation Id List By Role Id +GetNavigationIdListByRoleId(IMemoryCache _memoryCache, int roleId)

    public static List<int> GetNavigationIdListByRoleId(IMemoryCache memoryCache, int roleId)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}_{roleId}";
        if (!memoryCache.TryGetValue(cacheName, out List<int> list))
        {
            list = new List<int>();
            var rolePermissionList = GetRolePermissionList(memoryCache).Where(x => roleId == x.RoleId).ToList();
            var navigationList = GetNavigationList(memoryCache);
            foreach (var navigation in navigationList.Where(x => x.ParentId == 0).ToList())
            foreach (var childNavigation in navigationList.Where(x => x.ParentId == navigation.Id).ToList())
                if (rolePermissionList.Exists(r =>
                        r.TableName.Equals(nameof(Navigation), StringComparison.OrdinalIgnoreCase)
                        && r.ColumnId == childNavigation.Id))
                {
                    list.Add(childNavigation.Id);
                    if (!list.Contains(navigation.Id)) list.Add(navigation.Id);
                }
        }

        return list;
    }

    #endregion

    #region Admin қатарын алу +GetAllAdminList(IMemoryCache _memoryCache)

    public static List<Admin> GetAllAdminList(IMemoryCache memoryCache)
    {
        var cacheName = $"{MethodBase.GetCurrentMethod().Name}";
        if (!memoryCache.TryGetValue(cacheName, out List<Admin> adminList))
        {
            using var connection = Utilities.GetOpenConnection();
            adminList = connection.GetList<Admin>("where 1 = 1 ").ToList();
            memoryCache.Set(cacheName, adminList, TimeSpan.FromDays(10));
        }

        return adminList ?? new List<Admin>();
    }

    #endregion

    #region Check Navigation Permission +CheckNavigationPermission(IMemoryCache _memoryCache, List<int> roleIdList, Controller controller, Action action, string method, out bool canView, out bool canCreate, out bool canEdit, out bool canDelete)

    public static void CheckNavigationPermission(IMemoryCache memoryCache, List<int> roleIdList, Controller controller,
        ControllerActionDescriptor action, string method, out bool canView, out bool canCreate, out bool canEdit,
        out bool canDelete)
    {
        roleIdList ??= new List<int>();

        var actionName = action.ActionName.ToLower();
        var controllerName = action.ControllerName.ToLower();
        var controllerAttributes = controller.GetType().GetCustomAttribute<NoRoleAttribute>(false);
        var actionAttributes = action.MethodInfo.GetCustomAttributes<NoRoleAttribute>(false);
        if (controllerAttributes != null || (actionAttributes != null && actionAttributes.Any()))
        {
            canView = canCreate = canEdit = canDelete = true;
            return;
        }

        if (method.Equals("POST"))
        {
            if (actionName.StartsWith("get") && actionName.EndsWith("list"))
                actionName = actionName[3..^4];
            else if (actionName.StartsWith("set") && actionName.EndsWith("status")) actionName = actionName[3..^6];
        }

        var url = $"/{controllerName}/{actionName}/list";
        var navigationId = GetNavigationList(memoryCache).FirstOrDefault(x => x.NavUrl.Equals(url))?.Id ?? 0;

        canView = canCreate = canEdit = canDelete = false;
        if (navigationId == 0) return;

        var viewPermissionId = GetPermissionList(memoryCache)
            .FirstOrDefault(x => x.ManageType.Equals("view", StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        var createPermissionId = GetPermissionList(memoryCache)
            .FirstOrDefault(x => x.ManageType.Equals("create", StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        var editPermissionId = GetPermissionList(memoryCache)
            .FirstOrDefault(x => x.ManageType.Equals("edit", StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        var deletePermissionId = GetPermissionList(memoryCache)
            .FirstOrDefault(x => x.ManageType.Equals("delete", StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

        canView = GetRolePermissionList(memoryCache).Exists(x =>
            x.TableName.Equals(nameof(Navigation)) && x.ColumnId == navigationId && roleIdList.Contains(x.RoleId) &&
            x.PermissionId == viewPermissionId);
        canCreate = GetRolePermissionList(memoryCache).Exists(x =>
            x.TableName.Equals(nameof(Navigation)) && x.ColumnId == navigationId && roleIdList.Contains(x.RoleId) &&
            x.PermissionId == createPermissionId);
        canEdit = GetRolePermissionList(memoryCache).Exists(x =>
            x.TableName.Equals(nameof(Navigation)) && x.ColumnId == navigationId && roleIdList.Contains(x.RoleId) &&
            x.PermissionId == editPermissionId);
        canDelete = GetRolePermissionList(memoryCache).Exists(x =>
            x.TableName.Equals(nameof(Navigation)) && x.ColumnId == navigationId && roleIdList.Contains(x.RoleId) &&
            x.PermissionId == deletePermissionId);
    }

    #endregion

}