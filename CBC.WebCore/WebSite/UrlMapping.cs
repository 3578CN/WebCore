using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// URL 映射类。
    /// </summary>
    public class UrlMapping
    {
        internal const string Url_Mapping_Key = "Url_Mapping";

        /// <summary>
        /// 构造函数，创建 PageConfig 实例。
        /// </summary>
        public UrlMapping()
        {
            // 创建 PageConfig 实例。
            PageConfig = new PageConfig();
        }

        /// <summary>
        /// URL 匹配正则表达式字符串。
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 页面处理程序配置。
        /// </summary>
        public PageConfig PageConfig { get; private set; }

        /// <summary>
        /// 获取当前请求的 URL 映射。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回当前请求的 URL 映射。</returns>
        public static UrlMapping GetCurrent(HttpContext context)
        {
            if (context.Items[Url_Mapping_Key] == null)
            {
                context.Items.Add(Url_Mapping_Key, UrlMappingConfig.GetCurrent(context).GetUrlMapping(context));
            }
            return context.Items[Url_Mapping_Key] as UrlMapping;
        }
    }
}