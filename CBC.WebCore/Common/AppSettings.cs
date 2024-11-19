using CBC.WebCore.WebSite.Update.Entity;
using Microsoft.Extensions.Configuration;

namespace CBC.WebCore.Common
{
    /// <summary>
    /// 提供获取应用程序配置数据的方法类。
    /// </summary>
    /// <remarks>
    /// 构造函数，使用依赖注入的 IConfiguration 初始化配置。
    /// </remarks>
    /// <param name="configuration">IConfiguration 实例。</param>
    public class AppSettings(IConfiguration configuration)
    {
        private static AppSettings _instance;
        private static readonly object _lock = new();
        private IDictionary<string, string> _routing;
        private IDictionary<string, string> _connectionStrings;
        private string _templatePath;
        private string _pagerTemplatePath;
        private Security.Entity.Security _security;
        private AutoUpdate _autoUpdate;

        #region 静态属性

        /// <summary>
        /// 获取当前应用程序的配置实例（单例模式）。
        /// </summary>
        public static AppSettings Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            throw new InvalidOperationException("AppSettings 实例尚未初始化。请确保在程序启动时进行正确配置。");
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 初始化当前应用程序的配置实例。
        /// </summary>
        /// <param name="configuration">IConfiguration 实例。</param>
        public static void Initialize(IConfiguration configuration)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new AppSettings(configuration);
                    _instance.LoadSettings(); // 加载配置信息。
                }
            }
        }

        #endregion

        #region 属性

        /// <summary>
        /// 获取网站路由信息。
        /// </summary>
        public IDictionary<string, string> Routing => _routing;

        /// <summary>
        /// 获取页面模板根路径。
        /// </summary>
        public string TemplatePath => _templatePath;

        /// <summary>
        /// 获取分页模板根路径。
        /// </summary>
        public string PagerTemplatePath => _pagerTemplatePath;

        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        public IDictionary<string, string> ConnectionStrings => _connectionStrings;

        /// <summary>
        /// 获取安全验证信息。
        /// </summary>
        public Security.Entity.Security Security => _security;

        /// <summary>
        /// 获取静态页文件自动更新信息。
        /// </summary>
        public AutoUpdate AutoUpdate => _autoUpdate;

        #endregion

        #region 方法

        /// <summary>
        /// 从 IConfiguration 加载应用程序配置数据。
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // 加载 Routing 配置。
                // 从 webSettings:routing 节点中读取站点的 host 和 configFile 配置，并存入字典中。
                _routing = configuration.GetSection("webSettings:routing")
                    .GetChildren()
                    .ToDictionary(
                        x => x["host"], // 站点的 host 名称。
                        x => x["configFile"] // 对应的配置文件路径。
                    );

                // 加载模板根路径。
                // 从 webSettings:templates 节点中获取 templatePath 和 pagerTemplatePath 配置。
                _templatePath = configuration["webSettings:templates:templatePath"]; // 页面模板根路径。
                _pagerTemplatePath = configuration["webSettings:templates:pagerTemplatePath"]; // 分页模板根路径。

                // 加载数据库连接字符串配置。
                // 从 webSettings:databaseConnections 节点中读取数据库连接名称和连接字符串，并存入字典中。
                _connectionStrings = configuration.GetSection("webSettings:databaseConnections")
                    .GetChildren()
                    .ToDictionary(
                        x => x["name"], // 数据库连接名称。
                        x => x["connectionString"] // 数据库连接字符串。
                    );

                // 加载安全配置。
                // 包括登录路径、授权路径、无需授权路径、IP 配置和 Cookie 设置等。
                _security = new Security.Entity.Security(
                    configuration["security:type"], // 安全配置类型。
                    configuration["security:login:path"], // 登录页面路径。
                    configuration.GetSection("security:authorizedPaths").GetChildren()
                        .Select(x => x["path"]).ToList(), // 需要授权访问的路径列表。
                    configuration.GetSection("security:unrestrictedPaths").GetChildren()
                        .Select(x => x["path"]).ToList(), // 无需授权访问的路径列表。
                    configuration.GetSection("security:staticPageManagementIPs").GetChildren()
                        .ToDictionary(x => x["ip"], x => x["mask"]) // 静态页面管理的 IP 地址和对应的子网掩码。
                );

                // 加载自动更新配置。
                // 从 autoUpdate 节点中获取监视器和规则配置，并存入 AutoUpdateEntity 实体中。
                _autoUpdate = new AutoUpdate(
                    configuration.GetSection("autoUpdate:watchers").GetChildren()
                        .Select(x => new Watcher
                        {
                            EventTypes = x["eventTypes"], // 监视的事件类型。
                            Path = x["path"], // 监视的路径。
                            IncludeSubDirectories = bool.Parse(x["includeSubDirectories"]) // 是否包含子目录。
                        }).ToList(),
                    configuration.GetSection("autoUpdate:rules").GetChildren()
                        .Select(x => new Rule(x["path"])
                        {
                            Pattern = x["pattern"], // 匹配页面的正则表达式。
                            IncludeSubDirectories = bool.Parse(x["includeSubDirectories"]), // 是否包含子目录。
                            Interval = int.Parse(x["interval"]) // 自动更新的时间间隔（秒）。
                        }).ToList()
                );
            }
            catch (Exception ex)
            {
                // 捕获所有类型的异常，并输出异常信息。
                Console.WriteLine($"加载配置时发生错误: {ex.Message}");
                // 这里可以选择抛出异常或者处理异常。
                // throw;
            }
        }

        #endregion
    }
}