using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.Security
{
    /// <summary>
    /// 提供与 Cookie 相关的身份验证功能的静态类。
    /// </summary>
    public static class AuthVerification
    {
        /// <summary>
        /// 异步方法：登录指定用户并创建身份验证 Cookie。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文，包含有关当前请求和响应的信息。</param>
        /// <param name="userName">要登录的用户的用户名。</param>
        /// <param name="authProperties">身份验证属性，用于配置认证的相关参数，例如是否持久化 Cookie、过期时间等。</param>
        /// <returns>表示异步操作的任务。</returns>
        public static async Task SignInUserAsync(HttpContext context, string userName, AuthenticationProperties authProperties)
        {
            // 创建用户声明列表，包含用户的基本信息。
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userName)
            };

            // 使用声明创建用户身份，并将其与指定的认证方案关联。
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 使用 SignInAsync 方法生成并附加身份验证 Cookie。
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步方法：注销当前用户并删除身份验证 Cookie。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文，包含有关当前请求和响应的信息。</param>
        /// <returns>表示异步操作的任务。</returns>
        public static async Task SignOutUserAsync(HttpContext context)
        {
            // 使用 SignOutAsync 方法移除身份验证 Cookie。
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        }
    }
}