using CBC.WebCore.Common;
using CBC.WebCore.WebSite;
using CBC.WebCore.WebSite.Parsers;
using CBC.WebCore.WebSite.Parsers.TagParsers;

namespace CBC.TestApp.Handlers
{
    public class TestPageHandler : Parser
    {
        public override async Task Parse(HttpContext context)
        {
            //获取模板 Html 代码。
            PageHtml = await ResourceProcessor.LoadTemplateAsync(PageConfig.GetCurrent(context).Template);

            // 异步标签解析。
            await LoadTagParser.ParseAsync(PageHtml);
            await DataTagParser.ParseAsync(context, PageHtml);
        }
    }
}