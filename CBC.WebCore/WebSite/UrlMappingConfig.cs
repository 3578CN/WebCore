using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// URL 映射配置类。
    /// 如果有需要扩展，可以增加在站点路由配置 XML 文件中增加 URL 映射处理类配置，并在 SiteSettings.cs 中增加反射机制。
    /// </summary>
    public abstract class UrlMappingConfig
    {
        internal const string Url_Mapping_Config_Key = "Url_Mapping_Config";

        /// <summary>
        /// 获取当前请求的 URL 映射配置。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回当前请求的 URL 映射配置。</returns>
        public static UrlMappingConfig GetCurrent(HttpContext context)
        {
            if (context.Items[Url_Mapping_Config_Key] == null)
            {
                context.Items.Add(Url_Mapping_Config_Key, SiteSettings.GetCurrent(context).GetUrlMappingConfig());
            }
            return context.Items[Url_Mapping_Config_Key] as UrlMappingConfig;
        }

        /// <summary>
        /// 获取 URL 映射的实例。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回 URL 映射的实例。</returns>
        public abstract UrlMapping GetUrlMapping(HttpContext context);
    }
}