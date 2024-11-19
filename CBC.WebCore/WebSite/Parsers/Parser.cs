using System.Text;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite.Parsers
{
    /// <summary>
    /// 模板标签解析基类，用于处理自定义模板标签的解析逻辑。
    /// 继承此类的解析器类可以通过实现具体的解析逻辑，动态修改页面 HTML 内容。
    /// 此类还实现了 IHttpHandler 接口，用于处理 HTTP 请求，并将解析后的内容返回给客户端。
    /// </summary>
    public abstract class Parser
    {
        internal const string Page_Html_Key = "Page_Html";
        private StringBuilder _pageHtml = new();

        #region 静态属性

        /// <summary>
        /// 获取当前请求页面的 HTML 代码。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回页面的 HTML 代码字符串。如果未找到对应的 HTML 代码，则返回空字符串。</returns>
        public static string GetPageHtml(HttpContext context)
        {
            // 判断 context.Items 中是否存在指定的 Page_Html_Key，如果不存在则返回空字符串。
            if (context.Items[Page_Html_Key] == null)
            {
                return string.Empty;
            }

            // 返回存储在 context.Items 中与 Page_Html_Key 对应的 HTML 代码字符串。
            return context.Items[Page_Html_Key].ToString();
        }

        #endregion

        /// <summary>
        /// 设置或获取页面 HTML 代码。
        /// </summary>
        public StringBuilder PageHtml
        {
            get => _pageHtml;
            set => _pageHtml = value;
        }

        /// <summary>
        /// 解析页面。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        public abstract Task Parse(HttpContext context);

        /// <summary>
        /// 异步渲染页面的方法。此方法会解析当前请求的页面，并将页面的 HTML 内容绑定到 HttpContext 对象的 Items 集合中，以便在后续处理时使用。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文，其中包含请求和响应的相关信息。</param>
        /// <returns>返回包含页面 HTML 的 StringBuilder 对象，用于后续的渲染和输出。</returns>
        public async Task<StringBuilder> RenderAsync(HttpContext context)
        {
            // 解析当前请求的页面内容，并将解析后的内容存储在 _pageHtml 中。
            await Parse(context);

            // 将当前请求页面的 HTML 内容绑定到 HttpContext 的 Items 集合中，使用 Page_Html_Key 作为键值。
            context.Items[Page_Html_Key] = _pageHtml;

            // 返回包含页面 HTML 内容的 StringBuilder 对象。
            return _pageHtml;
        }
    }
}