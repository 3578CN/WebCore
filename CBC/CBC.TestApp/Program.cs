using CBC.TestApp.Endpoints;
using CBC.WebCore.Common;
using CBC.WebCore.Common.CacheHelpers;
using CBC.WebCore.Common.HttpHelpers;
using CBC.WebCore.Common.LogHelpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog 日志记录器。
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // 全局最低日志级别。
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // 覆盖 "Microsoft" 下的日志级别。
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}") // 输出到控制台。
    .WriteTo.File(path: "logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}") // 输出到文件。
    .CreateLogger();

// 将 Serilog 注册为日志服务提供者。
builder.Host.UseSerilog();

// 初始化配置
var configuration = builder.Configuration;

// 初始化应用程序设置，传入配置对象。
AppSettings.Initialize(configuration);

// 注册服务到容器中。
builder.Services.AddMemoryCache();                  // 内存缓存服务。
builder.Services.AddResponseCaching();              // 响应缓存服务。
builder.Services.AddSingleton<CacheHelper>();       // 注册自定义缓存服务。
builder.Services.AddSingleton<HttpInitializer>();   // 注册 HttpInitializer 服务。

// 配置身份验证服务并使用 Cookie 进行认证。
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TestCookie";                         // 自定义 Cookie 名称。
        options.Cookie.HttpOnly = true;                             // 将 Cookie 标记为 HttpOnly。
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    // 始终要求使用 HTTPS 传输 Cookie。
        options.Cookie.Path = "/";                                  // Cookie 路径，默认根路径。
        options.Cookie.SameSite = SameSiteMode.Lax;                 // 防止 CSRF 攻击。
        options.Cookie.Domain = "";                                 // 当前域名使用。
        options.SlidingExpiration = false;                          // 禁用滑动过期。
    });

// 添加 SignalR 服务。
builder.Services.AddSignalR();

builder.Services.AddAuthorization(); // 添加授权服务。

// 构建应用程序。
var app = builder.Build();

// 获取 ILoggerFactory 实例。
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

// 配置 LoggerHelper，使用工厂配置应用程序日志记录。
LoggerHelper.Configure(loggerFactory);

// 配置 HTTP 请求管道。
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // 开发环境使用异常页面。
}
else
{
    app.UseHsts(); // 生产环境使用 HSTS。
}

// 使用 X-Forwarded-For 和 X-Forwarded-Proto 头。
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 使用 HTTPS 重定向。
app.UseHttpsRedirection();

// 静态页默认首页设置。
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);

// 静态页面支持。
app.UseStaticFiles();

// 使用响应缓存中间件。
app.UseResponseCaching();

// 初始化 HttpInitializer 服务。
app.Services.GetRequiredService<HttpInitializer>();

// 设置全局响应的 Content-Type 和编码，默认为 UTF-8 编码的 HTML。
app.Use(async (context, next) =>
{
    // 在响应未开始发送之前设置 Content-Type 头。
    if (!context.Response.HasStarted)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
    }
    await next();
});

// 启用路由。
app.UseRouting();

// 使用授权中间件，授权必须在路由之后。
app.UseAuthorization();

// 注册 CBC 安全中间件。
// app.UseCBCSecurity();

// 使用顶级路由注册 SignalR 路由。
app.MapHub<ChatHub>("/chatHub");

// 注册 CBC 终结点路由中间件。
app.UseCBCEndpoints("Endpoints", "Index");

// 注册 CBC 静态页处理中间件。
app.UseCBCStaticPages();

app.Run();