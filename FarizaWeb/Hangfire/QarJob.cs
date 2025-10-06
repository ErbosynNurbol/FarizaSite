using System.Reflection;
using FarizaWeb.Caches;
using FarizaWeb.Controllers;
using COMMON;
using Dapper;
using DBHelper;
using Hangfire;
using MODEL;
using MODEL.ViewModels;
using Serilog;

namespace FarizaWeb.Hangfire;

public class QarJob
{
    private readonly IWebHostEnvironment _environment;
    private readonly IMemoryCache _memoryCache;
    private readonly Dictionary<string, string> _translationCache = new Dictionary<string, string>();
    public QarJob(IMemoryCache memoryCache, IWebHostEnvironment environment)
    {
        _memoryCache = memoryCache;
        _environment = environment;
    }

    #region Delete Old Log Files +JobDeleteOldLogFiles()

    public void JobDeleteOldLogFiles()
    {
        var key = MethodBase.GetCurrentMethod().Name;
        if (QarSingleton.GetInstance().GetRunStatus(key)) return;
        QarSingleton.GetInstance().SetRunStatus(key, true);
        try
        {
            var logDirectoryPath = _environment.ContentRootPath +
                                   (_environment.ContentRootPath.EndsWith("/") ? "" : "/") + "logs";
            var directory = new DirectoryInfo(logDirectoryPath);
            if (!directory.Exists) return;
            var txtFiles = directory.GetFiles("*.txt");
            foreach (var file in txtFiles)
            {
                var timeDifference = DateTime.Now - file.CreationTime;
                if (timeDifference.Days > 7) file.Delete();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "jobDeleteOldLogFiles");
        }
        finally
        {
            QarSingleton.GetInstance().SetRunStatus(key, false);
        }
    }

    #endregion

    #region Job Save Relogin AdminIds +JobSaveReloginAdminIds()

    public void JobSaveReloginAdminIds()
    {
        var key = MethodBase.GetCurrentMethod().Name;
        if (QarSingleton.GetInstance().GetRunStatus(key)) return;
        QarSingleton.GetInstance().SetRunStatus(key, true);
        try
        {
            using var connection = Utilities.GetOpenConnection();
            var reloginAdminList = connection.GetList<Admin>("where reLogin = 1").ToList();
            foreach (var reloginAdmin in reloginAdminList)
                QarSingleton.GetInstance().AddReLoginAdmin(reloginAdmin.Id, reloginAdmin.UpdateTime);
        }
        catch (Exception ex)
        {
            Log.Error(ex, key);
        }
        finally
        {
            QarSingleton.GetInstance().SetRunStatus(key, false);
        }
    }

    #endregion
}