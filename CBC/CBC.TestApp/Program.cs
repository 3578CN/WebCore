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

// ���� Serilog ��־��¼����
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // ȫ�������־����
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // ���� "Microsoft" �µ���־����
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}") // ���������̨��
    .WriteTo.File(path: "logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}") // ������ļ���
    .CreateLogger();

// �� Serilog ע��Ϊ��־�����ṩ�ߡ�
builder.Host.UseSerilog();

// ��ʼ������
var configuration = builder.Configuration;

// ��ʼ��Ӧ�ó������ã��������ö���
AppSettings.Initialize(configuration);

// ע����������С�
builder.Services.AddMemoryCache();                  // �ڴ滺�����
builder.Services.AddResponseCaching();              // ��Ӧ�������
builder.Services.AddSingleton<CacheHelper>();       // ע���Զ��建�����
builder.Services.AddSingleton<HttpInitializer>();   // ע�� HttpInitializer ����

// ���������֤����ʹ�� Cookie ������֤��
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TestCookie";                         // �Զ��� Cookie ���ơ�
        options.Cookie.HttpOnly = true;                             // �� Cookie ���Ϊ HttpOnly��
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    // ʼ��Ҫ��ʹ�� HTTPS ���� Cookie��
        options.Cookie.Path = "/";                                  // Cookie ·����Ĭ�ϸ�·����
        options.Cookie.SameSite = SameSiteMode.Lax;                 // ��ֹ CSRF ������
        options.Cookie.Domain = "";                                 // ��ǰ����ʹ�á�
        options.SlidingExpiration = false;                          // ���û������ڡ�
    });

// ��� SignalR ����
builder.Services.AddSignalR();

builder.Services.AddAuthorization(); // �����Ȩ����

// ����Ӧ�ó���
var app = builder.Build();

// ��ȡ ILoggerFactory ʵ����
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

// ���� LoggerHelper��ʹ�ù�������Ӧ�ó�����־��¼��
LoggerHelper.Configure(loggerFactory);

// ���� HTTP ����ܵ���
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // ��������ʹ���쳣ҳ�档
}
else
{
    app.UseHsts(); // ��������ʹ�� HSTS��
}

// ʹ�� X-Forwarded-For �� X-Forwarded-Proto ͷ��
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// ʹ�� HTTPS �ض���
app.UseHttpsRedirection();

// ��̬ҳĬ����ҳ���á�
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);

// ��̬ҳ��֧�֡�
app.UseStaticFiles();

// ʹ����Ӧ�����м����
app.UseResponseCaching();

// ��ʼ�� HttpInitializer ����
app.Services.GetRequiredService<HttpInitializer>();

// ����ȫ����Ӧ�� Content-Type �ͱ��룬Ĭ��Ϊ UTF-8 ����� HTML��
app.Use(async (context, next) =>
{
    // ����Ӧδ��ʼ����֮ǰ���� Content-Type ͷ��
    if (!context.Response.HasStarted)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
    }
    await next();
});

// ����·�ɡ�
app.UseRouting();

// ʹ����Ȩ�м������Ȩ������·��֮��
app.UseAuthorization();

// ע�� CBC ��ȫ�м����
// app.UseCBCSecurity();

// ʹ�ö���·��ע�� SignalR ·�ɡ�
app.MapHub<ChatHub>("/chatHub");

// ע�� CBC �ս��·���м����
app.UseCBCEndpoints("Endpoints", "Index");

// ע�� CBC ��̬ҳ�����м����
app.UseCBCStaticPages();

app.Run();