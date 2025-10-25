namespace CBC.TestApp.Endpoints
{
    public class Index : WebCore.WebView.Page
    {
        protected void LoadAsync(HttpContext context)
        {
            context.Response.WriteAsync("根目录首页");
        }
    }
}
