using CBC.WebCore.Common.LogHelpers;
using CBC.WebCore.WebView;

namespace CBC.TestApp.Endpoints.Account
{
    public class Add : Page
    {
        protected Task LoadAsync()
        {
            // 实现页面加载时的逻辑
            Main.Template = "Account/add.html";
            IncludeExecutionTime = true;

            LoggerHelper.Information("首页已打开。");
            return Task.CompletedTask;
        }
    }
}