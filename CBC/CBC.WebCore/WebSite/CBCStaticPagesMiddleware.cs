using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.Common.FileHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// 静态页处理中间件。
    /// </summary>
    public class CBCStaticPagesMiddleware(RequestDelegate next)
    {
        /// <summary>
        /// 用于保存客户端缓存的页面 URL 和最后更新时间的缓存列表。
        /// </summary>
        public static readonly ConcurrentDictionary<string, string> CachePageList = new();

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
                await next(context);

                // 终止后续处理，防止重复传递。
                return;
            }

            // 启动计时器。
            var stopwatch = Stopwatch.StartNew();

            #region 缓存检查（客户端按 F5 刷新会进到这里进行检查缓存是否有效），如果缓存命中或者页面没有更新，结束请求处理

            if (ValidateCache(context))
            {
                // 如果缓存命中并且页面没有更新，结束请求处理，防止继续处理其他中间件。
                return;
            }

            #endregion

            #region 清空、删除处理，完成后结束请求处理

            // 静态页 URL 的扩展名是 .htmln，进行静态页文件内容清空处理。
            if (context.GetRequestMode() == RequestMode.Null)
            {
                Page_Null(context); // 调用清空页面的方法。
                return; // 结束请求处理，防止继续处理其他中间件。
            }

            // 静态页 URL 的扩展名是 .htmld，进行静态页文件删除处理。
            if (context.GetRequestMode() == RequestMode.Delete)
            {
                Page_Delete(context); // 调用删除页面的方法。
                return; // 结束请求处理，防止继续处理其他中间件。
            }

            #endregion

            #region 发布、更新、保存处理

            // 静态页 URL 的扩展名是 .html，进行静态页文件首次请求的发布处理。
            if (context.GetRequestMode() == RequestMode.Publish)
            {
                Page_FirstPublish(context); // 调用首次请求的发布页面的方法。
            }

            // 静态页 URL 的扩展名是 .htmlu，进行静态页文件更新处理。
            if (context.GetRequestMode() == RequestMode.Update)
            {
                Page_Update(context); // 调用更新页面的方法。
            }

            // 静态页 URL 的扩展名是 .htmls，进行静态页文件保存处理。
            if (context.GetRequestMode() == RequestMode.Save)
            {
                Page_Save(context); // 调用保存页面的方法。
            }

            #endregion

            #region 渲染页面并输出到客户端

            // 获取页面的渲染内容，并计算 ETag 和 Last-Modified 值。
            var pageHandler = PageConfig.GetCurrent(context).GetTagParserType();
            var content = await pageHandler.RenderAsync(context);

            // 停止计时器。
            stopwatch.Stop();

            // 计算请求耗时（以毫秒为单位）。
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // 在响应末尾追加耗时信息。
            content.Append($"\n<!-- 请求耗时: {elapsedMilliseconds} 毫秒 -->");

            var contentBytes = Encoding.UTF8.GetBytes(content.ToString());

            // 设置响应内容类型为 HTML。
            context.Response.ContentType = "text/html";
            var responseStream = context.Response.Body;

            // 将内容写入到响应流中，并发送给客户端。
            await responseStream.WriteAsync(contentBytes.AsMemory(0, contentBytes.Length));
            await responseStream.FlushAsync(); // 刷新响应流，确保数据发送到客户端。

            #endregion

            // 调用下一个中间件。 
            await next(context);
        }

        #region 验证缓存是否命中

        /// <summary>
        /// 验证页面是否有更新或缓存是否命中。如果页面未更新且缓存命中，则返回 304 Not Modified 状态码，
        /// 客户端可以继续使用缓存的资源。如果页面有更新或缓存未命中，则返回 false。
        /// </summary>
        /// <param name="context">HTTP 请求的上下文。</param>
        /// <returns>如果页面未更新且缓存命中，返回 true；否则返回 false。</returns>
        private static bool ValidateCache(HttpContext context)
        {
            // 只有页面请求为发布模式，才需要缓存检查。
            if (context.GetRequestMode() == RequestMode.Publish)
            {
                // 尝试在页面缓存列表中查找已发布的页面，如果找到，说明页面不是首次发布，需要缓存检查。
                if (CachePageList.TryGetValue(context.GetAbsolutePath(), out var cachedETag))
                {
                    // 从请求头中获取 If-None-Match 来进行 eTag 比较，验证页面是否有更新。
                    var requestETag = context.Request.Headers.IfNoneMatch;

                    // 如果请求的 eTag 与缓存的 eTag 不一致，说明页面有更新，或页面缓存失败，返回 false。
                    if (requestETag != cachedETag)
                    {
                        requestETag = context.Request.Headers.IfNoneMatch.FirstOrDefault();
                        if (string.IsNullOrEmpty(requestETag))
                        {
                            Console.WriteLine("高频刷新导致请求头中 If-None-Match 为空。requestETag：" + requestETag);
                            // 发布失败。
                            // 之后进入首次请求的发布方法中，再次检查页面缓存列表中是否能找到，
                            // 如果能找到说明不是首次发布，这时就需要重新缓存页面。
                        }
                        return false; // 页面有更新。
                    }
                }

                // 尝试获取客户端传递的 If-Modified-Since 请求头。
                if (context.Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out var clientModifiedSince))
                {
                    // 获取当前页面的缓存时长（秒）。
                    var cacheDurationInSeconds = PageConfig.GetCurrent(context).CacheDuration;

                    // 设定当前时间作为缓存资源的最后修改时间。
                    var lastModifiedTime = DateTime.Now;

                    // 尝试解析客户端发送的 If-Modified-Since 头为日期格式。
                    if (DateTime.TryParse(clientModifiedSince, out var ifModifiedSinceDate))
                    {
                        // 如果缓存资源自 If-Modified-Since 之后没有过期，返回 304 状态码。
                        if ((lastModifiedTime - ifModifiedSinceDate).TotalSeconds < cacheDurationInSeconds)
                        {
                            // 缓存仍然有效，返回 304 Not Modified，客户端继续使用缓存。
                            context.Response.StatusCode = StatusCodes.Status304NotModified;
                            return true; // 缓存命中。
                        }
                    }
                }
            }

            // 不是发布模式的请求或缓存未命中。
            return false;
        }

        #endregion

        #region 静态页处理方法

        // 静态页文件内容清空处理。
        private static void Page_Null(HttpContext context)
        {
            using var fs = new FileStream(context.GetAbsolutePath(true), FileMode.Truncate);
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.WriteAsync($"{context.GetAbsolutePath(true)} 文件已清空！");
        }

        // 静态页文件删除处理。
        private static void Page_Delete(HttpContext context)
        {
            var isDelete = FileHelper.DeleteFile(context.GetAbsolutePath(true));

            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.WriteAsync(isDelete ? $"{context.GetAbsolutePath(true)} 文件已删除！" : $"{context.GetAbsolutePath(true)} 文件不存在！");
        }

        // 静态页文件首次请求的发布处理。
        private static void Page_FirstPublish(HttpContext context)
        {
            context.Response.Body = new PageFilter(context, context.Response.Body);

            // 如果请求的页面是 HTML 页面，并且该页面保存方法设置为缓存，那么就把该页面在客户端进行缓存。
            if (context.GetRequestFileExtension() == ".html" && PageConfig.GetCurrent(context).SaveMethod == SaveMethod.Cache)
            {
                // 尝试在页面缓存列表中查找已发布的页面，如果找到，说明页面并非首次请求的发布，如果找不到需要发送 ETag 头给客户端。
                if (CachePageList.TryGetValue(context.GetAbsolutePath(), out var cachedETag))
                {
                    // 在页面缓存列表中找到了，说明不是首次发布，有可能是客户端缓存失败了，需要重新缓存页面。

                    // 从缓存列表中找到 ETag 重新发送 ETag 头给客户端。
                    context.Response.Headers[HeaderNames.ETag] = cachedETag;
                }
                else
                {
                    // 缓存列表中没有找到，说明是首次请求的发布。

                    // 生成 ETag，这里使用了 GUID 作为 ETag。
                    var eTag = Guid.NewGuid().ToString();

                    // 添加到缓存页面列表。
                    CachePageList[context.GetAbsolutePath()] = eTag;

                    // 发送 ETag 头给客户端，用来验证页面是否有更新。
                    context.Response.Headers[HeaderNames.ETag] = eTag;
                }
                // 发送 Last-Modified 头给客户端，用于未来的 If-Modified-Since 请求，用来验证缓存是否过期。
                context.Response.Headers[HeaderNames.LastModified] = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);

                // 设置缓存头。
                var headers = context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(PageConfig.GetCurrent(context).CacheDuration),
                    NoCache = true, // 强制客户端在每次请求时都验证缓存。
                    MustRevalidate = true // 强制在缓存过期后进行重新验证。
                };

                // 设置 Vary 头以根据不同的请求头（如 Accept-Encoding）进行缓存区分。
                context.Response.Headers[HeaderNames.Vary] = "Accept-Encoding";
            }
        }

        // 静态页文件更新处理。
        private static void Page_Update(HttpContext context)
        {
            context.Response.Body = new PageFilter(context, context.Response.Body);

            if (PageConfig.GetCurrent(context).SaveMethod == SaveMethod.Cache)
            {
                // 生成新的 ETag，更新缓存页面列表。
                CachePageList[context.GetAbsolutePath()] = Guid.NewGuid().ToString();
            }
        }

        // 静态页文件保存处理。
        private static void Page_Save(HttpContext context)
        {
            PageConfig.GetCurrent(context).SaveMethod = SaveMethod.Save;
            context.Response.Body = new PageFilter(context, context.Response.Body);
        }

        #endregion
    }
}