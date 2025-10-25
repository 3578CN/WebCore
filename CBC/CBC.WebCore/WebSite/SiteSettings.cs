using System.Xml.Linq;
using CBC.WebCore.Common;
using CBC.WebCore.Common.CacheHelpers;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// 提供获取站点配置数据的方法类。
    /// </summary>
    public class SiteSettings
    {
        #region 属性

        /// <summary>
        /// 获取页面的字符编码格式。
        /// </summary>
        public string PageEncoding { get; private set; }

        /// <summary>
        /// 获取当请求未经授权（403 错误）时返回的错误页面路径。
        /// </summary>
        public string UnauthorizedErrorPage { get; private set; }

        /// <summary>
        /// 获取当请求的资源不存在或路径错误（404 错误）时返回的错误页面路径。
        /// </summary>
        public string NotFoundErrorPage { get; private set; }

        /// <summary>
        /// 获取当请求的静态页文件不存在（404 错误）时返回的错误页面路径。
        /// </summary>
        public string MissingFileErrorPage { get; private set; }

        /// <summary>
        /// 获取 URL 映射处理实例。
        /// </summary>
        public UrlMappingProcessor UrlMappingProcessor { get; private set; }

        #endregion

        #region 方法

        /// <summary>
        /// 获取当前请求的站点配置。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回当前请求的站点配置。</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static SiteSettings GetCurrent(HttpContext context)
        {
            // 获取请求的主机地址。
            var host = context.Request.Host.Host;

            // 根据请求的主机地址获取当前请求的站点配置文件路径。
            if (AppSettings.Current.Routing.TryGetValue(host, out string value))
            {
                // 从缓存中获取对象，如果不存在就添加到缓存。
                if (!CacheHelper.Current.Get(value, out SiteSettings cacheSiteSetting))
                {
                    // 初始化实例。
                    cacheSiteSetting = new SiteSettings();

                    // 读取站点配置信息。
                    cacheSiteSetting.Load(value);

                    //添加到缓存。
                    CacheHelper.Current.Set(value, cacheSiteSetting);
                }

                return cacheSiteSetting;
            }
            else
            {
                // 键不存在，处理未找到的情况。
                // 错误，根据请求的主机地址没有找到相对应的站点配置信息（属于非法的域名解析情况）。
                // 抛出异常，停止进一步的代码执行，并触发 ASP.NET Core 的异常处理机制。
                throw new InvalidOperationException("没有找到相对应的站点配置。");
            }
        }

        /// <summary>
        /// 获取 URL 映射配置实例。
        /// </summary>
        /// <returns>返回 URL 映射配置实例。</returns>
        public UrlMappingConfig GetUrlMappingConfig() => UrlMappingProcessor;

        #endregion

        #region 加载站点配置

        /// <summary>
        /// 加载站点配置。
        /// </summary>
        /// <param name="path">配置文件路径。</param>
        public void Load(string path)
        {
            // 【如果将来要修改为可扩展的 URL 映射处理，此处的实例要改成通过反射获取。】
            UrlMappingProcessor = new UrlMappingProcessor();

            if (AppSettings.Current.Routing.Count != 0)
            {
                // 加载 XML 文件。
                XDocument xmlDoc = XDocument.Load(path);

                // 直接读取并存储页面字符编码配置。
                PageEncoding = xmlDoc.Descendants("pageEncoding").FirstOrDefault()?.Attribute("value")?.Value;

                // 直接读取并存储错误页配置。
                var errorPagesElement = xmlDoc.Descendants("errorPages").FirstOrDefault();
                UnauthorizedErrorPage = errorPagesElement?.Attribute("unauthorizedErrorPage")?.Value;
                NotFoundErrorPage = errorPagesElement?.Attribute("notFoundErrorPage")?.Value;
                MissingFileErrorPage = errorPagesElement?.Attribute("missingFileErrorPage")?.Value;

                // 直接读取并存储 URL 映射设置。
                foreach (var page in xmlDoc.Descendants("page"))
                {
                    var urlMapping = new UrlMapping();
                    {
                        urlMapping.Pattern = page.Attribute("pattern")?.Value;
                        urlMapping.PageConfig.Template = page.Attribute("template")?.Value;
                        urlMapping.PageConfig.PagerTemplate = page.Attribute("pagerTemplate")?.Value;
                        urlMapping.PageConfig.HandlerType = page.Attribute("handlerType")?.Value;

                        // 尝试解析枚举类型，进行匹配。
                        if (Enum.TryParse<SaveMethod>(page.Attribute("saveMethod")?.Value, ignoreCase: true, out SaveMethod parsedMethod)
                            && Enum.IsDefined(typeof(SaveMethod), parsedMethod))
                        {
                            urlMapping.PageConfig.SaveMethod = parsedMethod;
                        }

                        urlMapping.PageConfig.CacheDuration = int.Parse(page.Attribute("cacheDuration")?.Value);
                        urlMapping.PageConfig.Values = new Dictionary<string, string>();
                    };

                    // 获取参数节点列表。
                    var paramNodes = page.Descendants("parameter");

                    // 构建参数字典。
                    var paramDict = paramNodes.ToDictionary(
                        param => param.Attribute("name")?.Value,
                        param => param.Attribute("value")?.Value
                    );

                    // 将参数字典赋值给 urlMapping.PageConfig.Values。
                    urlMapping.PageConfig.Values = paramDict;

                    UrlMappingProcessor.UrlMappingList.Add(urlMapping);
                }
            }
        }

        #endregion
    }
}