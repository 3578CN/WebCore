using CBC.WebCore.Handlers;
using CBC.WebCore.Security;
using Microsoft.AspNetCore.Authentication;

namespace CBC.TestApp.Endpoints.Account
{
    public class LoginAuth : IHttpHandler
    {
        public async Task ProcessRequestAsync(HttpContext context)
        {
            var username = context.Request.Query["username"];

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = true,                              // 允许在会话期间刷新身份验证票据。
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(1),   // 设置票据的过期时间为1分钟后。
                IsPersistent = true,                                // 持久化Cookie，即使关闭浏览器也会保持登录状态。
                IssuedUtc = DateTimeOffset.UtcNow,                  // 设置票据的生成时间为当前时间。
                RedirectUri = "/Account/list",                      // 登录成功后重定向的 URI。
            };

            // 调用 SignInUserAsync 方法。
            await AuthVerification.SignInUserAsync(context, username, authProperties);
        }

        public bool IsReusable
        {
            get => false;
        }
    }
}