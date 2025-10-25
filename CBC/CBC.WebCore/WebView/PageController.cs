using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace CBC.WebCore.WebView
{
    /// <summary>
    /// 页面控制器类，用于处理动态页面的渲染逻辑，继承自 ControllerBase。
    /// </summary>
    public class PageController : ControllerBase
    {
        private readonly Control _main = new();
        private bool _includeExecutionTime = false;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew(); // 启动计时器。
        private string _html = string.Empty;

        #region 属性

        /// <summary>
        /// 动态页面主控件。
        /// </summary>
        protected Control Main => _main;

        /// <summary>
        /// 是否在页面 HTML 代码结尾加入页面执行时间。
        /// 如果为 true，则在页面输出的末尾添加 <!--总耗时：{毫秒}-->。
        /// </summary>
        protected bool IncludeExecutionTime
        {
            set => _includeExecutionTime = value;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 异步渲染动态页面内容。
        /// </summary>
        /// <returns>返回一个表示异步操作的 Task 对象，Task 结果包含渲染后的动态页面 HTML 代码。</returns>
        public virtual async Task<StringBuilder> RenderAsync()
        {
            // 从主控件异步获取页面代码。
            _html = await _main.GetControlHtmlAsync();

            var html = new StringBuilder(_html);

            // 停止计时器。
            _stopwatch.Stop();

            // 是否在页面 HTML 代码结尾加入页面执行时间。
            if (_includeExecutionTime)
            {
                // 计算请求耗时（以毫秒为单位）。
                var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
                html.Append($"\n<!-- 请求耗时: {elapsedMilliseconds} 毫秒 -->");
            }

            return html;
        }

        /// <summary>
        /// 异步渲染并返回视图的内容结果。
        /// 该方法调用异步渲染方法，并将渲染后的结果转换为字符串， 
        /// 作为 ContentResult 返回给客户端。 
        /// </summary>
        /// <returns>一个包含渲染视图内容的 ContentResult 对象。</returns>
        public virtual async Task<ContentResult> ViewAsync()
        {
            return Content((await RenderAsync()).ToString());
        }

        #endregion
    }
}