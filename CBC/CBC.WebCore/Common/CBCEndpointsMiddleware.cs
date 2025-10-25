using System.Reflection;
using System.Text;
using CBC.WebCore.Common.CacheHelpers;
using CBC.WebCore.Handlers;
using CBC.WebCore.WebSite;
using CBC.WebCore.WebView;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.Common
{
    /// <summary>
    /// 终结点路由中间件，用于处理不同的请求类型并将其路由到相应的处理程序。
    /// </summary>
    public class CBCEndpointsMiddleware
    {
        private readonly RequestDelegate _next; // 下一个中间件的委托。
        private readonly CacheHelper _cacheService; // 用于缓存页面类型的缓存服务。
        private readonly string _endpointsDirectory; // 请求终结点的存放目录。
        private readonly string[] _defaultHomePages; // 动态页的默认首页集合。
        private readonly Dictionary<Type, Func<Type, HttpContext, Task>> _handlers; // 接口类型与处理方法的映射字典。
        private static readonly char[] _splitChars = ['/'];

        /// <summary>
        /// 构造函数，初始化中间件的必要组件。
        /// </summary>
        /// <param name="next">下一个中间件的委托。</param>
        /// <param name="cacheService">用于缓存页面类型的缓存服务。</param>
        /// <param name="endpointsDirectory">设置请求终结点的存放目录。</param>
        /// <param name="defaultHomePages">动态页的默认首页集合。</param>
        public CBCEndpointsMiddleware(RequestDelegate next, CacheHelper cacheService, string endpointsDirectory, params string[] defaultHomePages)
        {
            _next = next;
            _cacheService = cacheService;
            _endpointsDirectory = endpointsDirectory;
            _defaultHomePages = defaultHomePages;

            // 初始化接口类型与处理方法的映射关系。
            _handlers = new Dictionary<Type, Func<Type, HttpContext, Task>>
            {
                { typeof(IPage), HandlePageRequest },
                { typeof(IHttpHandler), HandleHttpHandlerRequest },
                { typeof(IJsonHttpHandler), HandleJsonHttpHandlerRequest }
            };
        }

        /// <summary>
        /// 中间件的核心调用方法，处理 HTTP 请求。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // 检查是否已经匹配到终结点。
            if (context.GetEndpoint() != null)
            {
                // 如果匹配到终结点，传递请求给下一个中间件。
                await _next(context);

                // 终止后续处理，防止重复传递。
                return;
            }

            #region 终结点路由方法

            if (context.GetRequestType() == RequestType.DynamicPage)
            {
                // 获取请求的路径，并在前面加上请求终结点的目录路径。
                var path = $"{_endpointsDirectory}{context.Request.Path.Value}";

                // 使用 Split 分割路径，过滤掉空的部分，并用 . 连接（去掉多余的 /）。
                path = string.Join(".", path.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries));

                // 检查缓存中是否存在对应路径的类型。
                if (!_cacheService.Get(path, out Type targetType))
                {
                    // 获取当前类的完全限定名。
                    var currentTypeFullName = $"{AppDomain.CurrentDomain.FriendlyName}.{path}";

                    // 获取当前应用程序的入口程序集（即执行的主要程序集）。
                    var entryAssembly = Assembly.GetEntryAssembly();

                    // 尝试从 entryAssembly 获取目标类型，如果找到了匹配的类型，则赋值给 targetType。
                    // 其中：
                    // - currentTypeFullName 是要查找的类型的全名，包括命名空间。
                    // - throwOnError: false 表示如果类型找不到，不会抛出异常，而是返回 null。
                    // - ignoreCase: true 表示类型名称的查找是不区分大小写的。
                    targetType = entryAssembly?.GetType(currentTypeFullName, throwOnError: false, ignoreCase: true);

                    // 如果以上尝试未找到目标类型，则遍历 _defaultHomePages 并继续尝试获取目标类型。
                    if (targetType == null)
                    {
                        // 使用 Lambda 表达式替代 foreach 循环，尝试从 _defaultHomePages 中查找目标类型。
                        targetType = _defaultHomePages
                            // 使用 Select 方法遍历 _defaultHomePages 集合中的每个元素 defaultHomePage。
                            .Select(defaultHomePage =>
                                // 使用 entryAssembly?.GetType 生成每个 defaultHomePage 对应的类型。
                                // 将 currentTypeFullName 和 defaultHomePage 拼接成完整的类型名称，忽略大小写，且不在未找到时抛出异常。
                                entryAssembly?.GetType($"{currentTypeFullName}.{defaultHomePage}", throwOnError: false, ignoreCase: true)
                            )
                            // 使用 FirstOrDefault 查找第一个不为 null 的类型，即找到目标类型后立即返回；如果没有找到则返回 null。
                            .FirstOrDefault(type => type != null);
                    }

                    if (targetType == null)
                    {
                        // 如果没有找到类型，返回自定义错误信息。
                        await context.Error404Async("没有找到页面类型！");
                        return;
                    }

                    // 将找到的页面类型缓存起来。
                    _cacheService.Set(path, targetType);
                }

                // 遍历接口处理程序字典，找到匹配的处理程序并执行。
                var handler = _handlers.FirstOrDefault(h => h.Key.IsAssignableFrom(targetType));
                if (handler.Value != null)
                {
                    await handler.Value(targetType, context);
                    return;
                }

                // 如果没有匹配的处理程序，返回 400 错误。
                await context.Error400Async("没有实现任何接口！");
            }

            #endregion

            // 调用下一个中间件。
            await _next(context);
        }

        #region 处理接口请求方法

        /// <summary>
        /// 处理 IPage 接口的请求。
        /// </summary>
        /// <param name="targetType">请求对应的页面类型。</param>
        /// <param name="context">当前的 HTTP 上下文。</param>
        private async Task HandlePageRequest(Type targetType, HttpContext context)
        {
            var page = (IPage)Activator.CreateInstance(targetType); // 创建页面实例。
            await page.InvokeLoadAsync(context); // 调用页面的加载方法。
            var content = await page.RenderAsync(); // 渲染页面内容。
            var contentBytes = Encoding.UTF8.GetBytes(content.ToString()); // 将内容转换为字节数组。

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "text/html"; // 设置响应的内容类型。
                context.Response.ContentLength = contentBytes.Length; // 设置响应的内容长度。
            }

            var responseStream = context.Response.Body; // 获取响应流。
            await responseStream.WriteAsync(contentBytes.AsMemory(0, contentBytes.Length), CancellationToken.None); // 写入响应内容。
            await responseStream.FlushAsync(); // 刷新流，将数据发送到客户端。
        }

        /// <summary>
        /// 处理 IHttpHandler 接口的请求。
        /// </summary>
        /// <param name="targetType">请求对应的处理程序类型。</param>
        /// <param name="context">当前的 HTTP 上下文。</param>
        private async Task HandleHttpHandlerRequest(Type targetType, HttpContext context)
        {
            var handler = (IHttpHandler)Activator.CreateInstance(targetType); // 创建处理程序实例。
            await handler.ProcessRequestAsync(context); // 处理 HTTP 请求。
        }

        /// <summary>
        /// 处理 IJsonHttpHandler 接口的请求。
        /// </summary>
        /// <param name="targetType">请求对应的处理程序类型。</param>
        /// <param name="context">当前的 HTTP 上下文。</param>
        private async Task HandleJsonHttpHandlerRequest(Type targetType, HttpContext context)
        {
            var handler = (IJsonHttpHandler)Activator.CreateInstance(targetType); // 创建处理程序实例。
            await handler.ProcessRequestAsync(context); // 处理 HTTP 请求。
        }
    }

    #endregion
}