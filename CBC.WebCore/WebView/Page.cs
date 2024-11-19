using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebView
{
    /// <summary>
    /// 动态页面渲染类。
    /// 继承此类的页面类可以实现动态页面的渲染和输出。
    /// </summary>
    public abstract class Page : IPage
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
        /// 异步调用实现类的 LoadAsync 或同步调用 Load 方法（如果存在）。
        /// 该方法通过显式接口实现 IPage 接口，使用反射机制查找并调用名为 "LoadAsync" 或 "Load" 的方法。
        /// 如果实现类中没有找到 "LoadAsync" 或 "Load" 方法，或者方法无效，则不会执行任何操作。
        /// </summary>
        /// <param name="context">
        /// 表示当前 HTTP 请求的上下文对象，包含请求、响应、用户信息等信息。
        /// 该参数将传递给 "LoadAsync" 或 "Load" 方法（如果该方法需要参数）。
        /// </param>
        /// <returns>
        /// 返回一个表示异步操作的 Task 对象，调用者可以等待该任务完成。
        /// 如果 "LoadAsync" 或 "Load" 方法不存在，返回的 Task 将立即完成，不执行任何操作。
        /// </returns>
        async Task IPage.InvokeLoadAsync(HttpContext context)
        {
            try
            {
                // 获取当前类中名为 "LoadAsync" 或 "Load" 的方法信息，搜索范围包括公共和非公共的实例方法。
                var asyncMethod = GetType().GetMethod("LoadAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var syncMethod = GetType().GetMethod("Load", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var method = asyncMethod ?? syncMethod; // 获取 "LoadAsync" 或 "Load" 方法，如果都不存在则为 null。
                if (method != null)
                {
                    // 准备方法参数，如果方法不需要参数则为 null。
                    var parameters = method.GetParameters().Length == 0 ? null : new object[] { context };

                    // 调用方法并获取返回结果。
                    var result = method.Invoke(this, parameters);

                    // 如果返回结果是 Task，则等待其完成。
                    if (result is Task task)
                    {
                        await task;
                    }
                }
                else
                {
                    // 如果没有找到任何方法，返回一个已完成的任务。
                    await Task.CompletedTask;
                }
            }
            catch (TargetInvocationException ex)
            {
                // 捕获通过反射调用方法时的异常，并抛出原始异常。
                throw ex.InnerException ?? ex;
            }
        }

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

        #endregion
    }
}