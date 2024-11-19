using CBC.WebCore.Security;
using CBC.WebCore.WebSite;
using Microsoft.AspNetCore.Builder;

namespace CBC.WebCore.Common
{
    /// <summary>
    /// 扩展方法用于将中间件添加到 HTTP 请求管道中。
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 将安全验证中间件添加到 HTTP 请求管道中。
        /// </summary>
        /// <param name="builder">应用程序构建器。</param>
        /// <returns>应用程序构建器。</returns>
        public static IApplicationBuilder UseCBCSecurity(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CBCSecurityMiddleware>();
        }

        /// <summary>
        /// 将终结点路由中间件添加到 HTTP 请求管道中。
        /// </summary>
        /// <param name="builder">应用程序构建器。</param>
        /// <param name="endpointsDirectory">设置请求终结点的存放目录。</param>
        /// <param name="defaultHomePages">动态页的默认首页集合。</param>
        /// <returns>应用程序构建器。</returns>
        public static IApplicationBuilder UseCBCEndpoints(
            this IApplicationBuilder builder,
            string endpointsDirectory,
            params string[] defaultHomePages)
        {
            // 将默认首页参数传递给中间件。
            return builder.UseMiddleware<CBCEndpointsMiddleware>(endpointsDirectory, defaultHomePages);
        }

        /// <summary>
        /// 将静态页处理中间件添加到 HTTP 请求管道中。
        /// </summary>
        /// <param name="builder">应用程序构建器。</param>
        /// <returns>应用程序构建器。</returns>
        public static IApplicationBuilder UseCBCStaticPages(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CBCStaticPagesMiddleware>();
        }
    }
}
