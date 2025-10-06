using System.Text.Encodings.Web;
using System.Text.Unicode;
using FarizaWeb;
using FarizaWeb.Filters;
using FarizaWeb.Hangfire;
using COMMON;
using Dapper;
using DBHelper;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.WebEncoders;
using Serilog;
using Serilog.Events;

var defaultTheme = "Fariza";
var redirectUrl = string.Empty;
string domain = null;
long maxFileSize = 2L * 1024 * 1024 * 1024;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: null)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
QarSingleton.GetInstance().SetSiteTheme(defaultTheme);
QarSingleton.GetInstance().SetConnectionString(builder.Configuration[$"{defaultTheme}:ConnectionString"]);
QarSingleton.GetInstance().SetSiteUrl(builder.Configuration[$"{defaultTheme}:SiteUrl"]);
redirectUrl = $"http://localhost:{builder.Configuration[$"{defaultTheme}:Port"]}";
if (string.IsNullOrEmpty(redirectUrl))
{
    throw new Exception($"RedirectUrl is empty defaultTheme ({defaultTheme})");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = new PathString("/kz/admin/login/");
    options.AccessDeniedPath = new PathString("/kz/admin/login/");
    options.LogoutPath = new PathString("/kz/admin/signout/");
    options.Cookie.Path = "/";
    options.SlidingExpiration = true;
    options.Cookie.Domain = domain;
    options.Cookie.Name = "qar_cookie";
    options.Cookie.HttpOnly = true;
});

builder.Services.AddControllersWithViews((configure =>
{
    configure.Filters.Add(typeof(PermissionFilter));
    configure.Filters.Add(typeof(QarFilter));
})).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressConsumesConstraintForFormFileParameters = true;
    options.SuppressInferBindingSourcesForParameters = true;
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = maxFileSize;
    o.ValueLengthLimit = int.MaxValue;
    o.ValueCountLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
    o.KeyLengthLimit = int.MaxValue;
});

builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});

builder.WebHost.ConfigureKestrel(opts =>
{
    opts.Limits.MaxRequestBodySize = maxFileSize;
    opts.Limits.MinRequestBodyDataRate = null;   
    opts.Limits.KeepAliveTimeout      = TimeSpan.FromMinutes(30);
    opts.Limits.RequestHeadersTimeout  = TimeSpan.FromMinutes(30);
});

builder.Services.AddHangfire(x => x.UseMemoryStorage());
builder.Services.AddTransient<QarJob>();
builder.Services.AddHangfireServer();


var app = builder.Build();
var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Remove(".xml");
provider.Mappings.Add(".xml", "application/xml");
provider.Mappings.Remove(".txt");
provider.Mappings.Add(".txt", "text/plain");
provider.Mappings.Remove(".xsl");
provider.Mappings.Add(".xsl", "text/xsl");
provider.Mappings.Remove(".exe");
provider.Mappings.Add(".exe", "application/exe");
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => { ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000"); }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "culture_action",
    pattern: "{culture=kz}/{action=Login}/{query?}",
    defaults: new { controller = "Admin" },
    constraints: new { culture = "kz|tote|latyn|ru|en" } //|ru|en|zh-cn|tr
);

// app.MapControllerRoute(
//     name: "language_default",
//     pattern: "{culture=kz}/{Admin}/{action=Login}/{query?}",
//     constraints: new { culture = "kz|tote|latyn|ru|en" } //|ru|en|zh-cn|tr
// );

app.MapControllerRoute(
    name: "admin_default",
    pattern: "{culture=kz}/{controller=Admin}/{action=Login}/{query?}",
    constraints: new { culture = "kz|tote|latyn|ru|en|zh-cn|tr" }
);

app.MapFallbackToFile("404.html");
app.UseHangfireDashboard();
SimpleCRUD.SetDialect(SimpleCRUD.Dialect.MySQL);
SimpleCRUD.SetTableNameResolver(new QarTableNameResolver());

if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{
    BackgroundJob.Schedule<QarJob>(q => q.JobSaveReloginAdminIds(), TimeSpan.FromMinutes(1));
    RecurringJob.AddOrUpdate<QarJob>("job_delete_old_log_files", q => q.JobDeleteOldLogFiles(), Cron.Daily); // 1 day
    app.Run(redirectUrl);
}