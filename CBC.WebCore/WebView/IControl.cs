namespace CBC.WebCore.WebView
{
    /// <summary>
    /// 动态页面控件接口。
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// 异步获取动态页面控件的 HTML 代码。
        /// </summary>
        /// <returns>返回包含动态页面控件 HTML 代码的异步任务。</returns>
        Task<string> GetControlHtmlAsync();
    }
}