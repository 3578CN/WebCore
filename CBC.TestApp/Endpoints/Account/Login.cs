using CBC.WebCore.Common;
using CBC.WebCore.WebView;

namespace CBC.TestApp.Endpoints.Account
{
    public class Login : Page
    {
        protected void LoadAsync(HttpContext context)
        {
            Main.Template = "Account/login.html";
            if (context.User.Identity.IsAuthenticated == true)
            {
                context.Location("/Account/list");
            }
        }
    }
}