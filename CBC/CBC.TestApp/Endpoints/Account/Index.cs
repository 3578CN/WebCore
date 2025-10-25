namespace CBC.TestApp.Endpoints.Account
{
    public class Index : WebCore.WebView.Page
    {
        protected void LoadAsync(HttpContext context)
        {
            context.Response.WriteAsync("Account 目录首页");
        }
    }
}
