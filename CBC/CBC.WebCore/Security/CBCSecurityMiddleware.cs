using CBC.WebCore.Common;
using CBC.WebCore.WebSite;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.Security
{
    /// <summary>
    /// 安全验证操作方法中间件。
    /// </summary>
    public class CBCSecurityMiddleware(RequestDelegate next)
    {
        // 安全检查委托，支持带有 ref 参数的方法。
        delegate void SecurityCheckDelegate(HttpContext context, AppSettings appsettings, ref bool isAuthorized);

        #region 安全检查处理逻辑

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var appsettings = AppSettings.Current;
            bool isAuthorized = false;

            //身份验证方法。
            // 创建一个包含 RequestType 与对应处理方法的字典。
            var requestTypeActions = new Dictionary<RequestType, SecurityCheckDelegate>
            {
                { RequestType.StaticPage, OnStaticPageSecurityCheck },
                { RequestType.DynamicPage, OnDynamicPageSecurityCheck }
            };

            // 检查当前请求类型，并执行对应的处理方法。
            if (requestTypeActions.TryGetValue(context.GetRequestType(), out var action))
            {
                action(context, appsettings, ref isAuthorized);
            }

            // 当通过验证或者当前页是登录页时，允许继续执行下一个中间件。
            if (isAuthorized || context.Request.Path == appsettings.Security.Login)
            {
                await next(context);
            }
        }

        #endregion

        // 静态页安全检查方法。
        private static void OnStaticPageSecurityCheck(HttpContext context, AppSettings appsettings, ref bool isAuthorized)
        {
            isAuthorized = true;
            //context.Error403Async("静态页操作权限不足！");
        }

        #region 动态页安全检查方法

        private static void OnDynamicPageSecurityCheck(HttpContext context, AppSettings appsettings, ref bool isAuthorized)
        {
            // 开始登录验证。

            // 获取当前请求的路径。
            var requestPath = context.Request.Path.ToString();

            // 检查请求路径是否需要授权。
            bool isAuthorizedPath = appsettings.Security.AuthorizedPaths
                .Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase));

            // 如果请求路径不在授权路径中，直接授权通过。
            if (!isAuthorizedPath)
            {
                isAuthorized = true;
                return;
            }

            // 如果用户已经登录，授权通过。
            if (context.User.Identity.IsAuthenticated)
            {
                isAuthorized = true;
                return;
            }

            // 检查请求路径是否为登录页面，如果是，授权通过。
            if (context.Request.Path == appsettings.Security.Login)
            {
                isAuthorized = true;
                return;
            }

            // 检查请求路径是否为无需授权即可访问的路径，如果是，授权通过。
            bool isUnrestrictedPath = appsettings.Security.UnrestrictedPaths
                .Any(path => requestPath.StartsWith(path, StringComparison.OrdinalIgnoreCase));
            if (isUnrestrictedPath)
            {
                isAuthorized = true;
                return;
            }

            // 如果到这里，说明用户未登录且请求的路径需要授权，跳转到登录页。
            if (!isAuthorized)
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Location($"{appsettings.Security.Login}?returnUrl={returnUrl}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 表示在安全性相关操作中发生的异常。用于处理在安全检查过程中发现的问题，例如权限不足或访问被拒绝。
    /// </summary>
    public class SecurityException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="SecurityException"/> 类的新实例，并指定错误信息。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public SecurityException(string message)
            : base(message)
        {
        }
    }
}