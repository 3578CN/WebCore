using System.Text.RegularExpressions;
using CBC.WebCore.Common;
using CBC.WebCore.Common.StringHelpers;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// URL 映射处理类。
    /// 此类继承 UrlMappingConfig 抽象类。
    /// 如果扩展 UrlMappingConfig 功能，该类需要通过反射机制获取类型。
    /// </summary>
    public class UrlMappingProcessor : UrlMappingConfig
    {
        /// <summary>
        /// URL 映射列表。
        /// </summary>
        public IList<UrlMapping> UrlMappingList { get; }

        /// <summary>
        /// 构造函数，初始化 UrlMappingList，创建一个 UrlMapping 对象的列表实例。
        /// </summary>
        public UrlMappingProcessor()
        {
            UrlMappingList = [];
        }

        #region 返回 URL 映射

        /// <summary>
        /// 获取 URL 映射的实例。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回 URL 映射的实例。</returns>
        public override UrlMapping GetUrlMapping(HttpContext context)
        {
            var urlMapping = new UrlMapping();
            foreach (UrlMapping var in UrlMappingList)
            {
                // 从 URL 中获取绝对路径，用于正则表达式的匹配。
                var path = context.GetAbsolutePath();
                Regex regex = new(var.Pattern + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                if (regex.IsMatch(path))
                {
                    // 是否仅使用字符串模板。
                    if (string.IsNullOrEmpty(var.PageConfig.Template))
                    {
                        urlMapping.PageConfig.UseStringTemplateOnly = true;
                    }
                    // 正则表达式。
                    urlMapping.Pattern = var.Pattern;
                    // 模板文件。
                    urlMapping.PageConfig.Template = var.PageConfig.Template;
                    // 翻页代码模板文件。
                    urlMapping.PageConfig.PagerTemplate = var.PageConfig.PagerTemplate;
                    // 页面缓存时间。
                    urlMapping.PageConfig.CacheDuration = var.PageConfig.CacheDuration;
                    // 页面处理对象类型。
                    if (!string.IsNullOrEmpty(var.PageConfig.HandlerType))
                    {
                        urlMapping.PageConfig.HandlerType = var.PageConfig.HandlerType;
                    }
                    // 页面保存方法。
                    urlMapping.PageConfig.SaveMethod = var.PageConfig.SaveMethod;
                    // Url 属性集合。
                    foreach (KeyValuePair<string, string> item in var.PageConfig.Values)
                    {
                        var value = RegexHelper.Replace(context.GetAbsolutePath(), urlMapping.Pattern, item.Value);
                        urlMapping.PageConfig.Values.Add(item.Key, value);
                    }
                    break;
                }
            }
            return urlMapping;
        }

        #endregion
    }
}