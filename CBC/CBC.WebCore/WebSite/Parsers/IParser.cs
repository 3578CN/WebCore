using System.Text;
using CBC.WebCore.WebSite.Parsers.TagsEntity;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite.Parsers
{
    /// <summary>
    /// 标签解析接口。定义了用于解析 HTML 页面中标签的标准方法，支持异步操作。实现此接口的类可以根据特定的标签代码，对 HTML 内容进行解析和处理。
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// 异步标签解析方法。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        /// <param name="dataTags">数据标签属性集合。</param>
        Task ParseAsync(HttpContext context, StringBuilder pageHtml, DataTags dataTags);
    }
}