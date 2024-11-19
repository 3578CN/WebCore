namespace CBC.TestApp.Endpoints
{
    public class Chat : CBC.WebCore.WebView.Page
    {
        protected void LoadAsync()
        {
            Main.Template = "chat.html";
        }
    }
}
