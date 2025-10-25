using CBC.WebCore.WebSite;

namespace CBC.TestApp.Endpoints
{
    public class Test : CBC.WebCore.WebView.Page
    {
        protected void LoadAsync(HttpContext context)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            context.Response.WriteAsync("缓存页面列表\r\n");
            var list = CBCStaticPagesMiddleware.CachePageList;
            foreach (var item in list)
            {
                context.Response.WriteAsync($"{item.Key}    {item.Value}\r\n");
            }
        }
    }
}
