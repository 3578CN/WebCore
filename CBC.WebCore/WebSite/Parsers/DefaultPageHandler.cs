using CBC.WebCore.WebSite.Parsers.TagParsers;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite.Parsers
{
    /// <summary>
    /// 如果在站点配置文件中未配置自定义处理程序类，那么使用默认页面处理类。
    /// 该类继承自 <see cref="Parser"/>，负责解析并处理页面的 HTML 内容。
    /// </summary>
    public class DefaultPageHandler : Parser
    {
        /// <summary>
        /// 解析页面的核心方法。
        /// 此方法将加载指定的模板文件，并对其进行标签解析和数据绑定。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文，包含请求和响应的所有信息。</param>
        public override async Task Parse(HttpContext context)
        {
            // 获取当前页面的配置，加载与页面对应的模板 HTML 代码。
            // 这里使用 PageConfig.GetCurrent(context) 来获取当前请求的配置，
            // 并通过 ResourceProcessor.LoadTemplateAsync 方法异步加载模板文件的内容。
            PageHtml = await Common.ResourceProcessor.LoadTemplateAsync(PageConfig.GetCurrent(context).Template);

            // 异步解析模板中的标签。
            await LoadTagParser.ParseAsync(PageHtml);
            await DataTagParser.ParseAsync(context, PageHtml);
        }
    }
}