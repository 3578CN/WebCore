using System.Text;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebView
{
    /// <summary>
    /// 定义动态页面的接口，提供页面加载和渲染的核心方法。
    /// 该接口旨在支持基于 HTTP 请求的页面处理，允许实现者定义页面的加载逻辑和渲染流程。
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// 委托执行页面的 LoadAsync 方法，并处理传入的 HTTP 上下文。
        /// 此方法用于在页面加载过程中处理请求数据，执行必要的初始化或数据绑定操作。
        /// </summary>
        /// <param name="context">表示当前请求的 HTTP 上下文，包含请求和响应的所有信息。</param>
        /// <returns>表示异步操作的 Task 对象。</returns>
        Task InvokeLoadAsync(HttpContext context);

        /// <summary>
        /// 渲染页面内容并异步返回渲染结果。
        /// 此方法用于生成最终的 HTML 内容，返回给客户端以供展示。
        /// </summary>
        /// <returns>返回一个包含渲染后页面的 HTML 字符串内容的 Task 对象。</returns>
        Task<StringBuilder> RenderAsync();
    }
}