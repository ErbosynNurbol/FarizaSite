using System.Web;

namespace COMMON;

public static class UrlHelper
{
    public static string SiteUrl => QarSingleton.GetInstance().GetSiteUrl();
    public static string DefaultAvatar => "/images/default_avatar.png";

    public static string GetAvatarUrl(string url = "") =>
        GetFullUrl(string.IsNullOrWhiteSpace(url) ? DefaultAvatar : url);

    public static string GetFullUrl(string path) => string.IsNullOrWhiteSpace(path)
        ? string.Empty
        : path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : PathHelper.Combine(SiteUrl, path);


    public static string AddParam(string url, string key, string value)
    {
        try
        {
            if (url.StartsWith('/'))
            {
                var uri = new Uri(PathHelper.Combine(SiteUrl, url));
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                queryParams[key] = value;
                var query = queryParams.ToString();
                return uri.AbsolutePath + (string.IsNullOrEmpty(query) ? "" : "?" + query);
            }
            else
            {
                var uriBuilder = new UriBuilder(new Uri(url));
                var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
                queryParams[key] = value;
                uriBuilder.Query = queryParams.ToString() ?? string.Empty;
                return uriBuilder.ToString();
            }
        }
        catch
        {
            return url;
        }
    }
}