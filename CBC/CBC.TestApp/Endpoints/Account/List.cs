using CBC.WebCore.Common;
using CBC.WebCore.WebView;
using CBC.WebCore.WebView.DataControl;

namespace CBC.TestApp.Endpoints.Account
{
    public class List : Page
    {
        protected async Task LoadAsync(HttpContext context)
        {
            // 实现页面加载时的逻辑
            Main.Template = "Account/list.html";

            //获取当前页码。
            var page = context.Request.Query["Page"];
            var thisPage = string.IsNullOrEmpty(page) ? 1 : Convert.ToInt32(page);

            var mysql = new WebCore.Common.DataHelpers.MySqlHelper(AppSettings.Current.ConnectionStrings["AppTestDB"]);
            mysql.Parameters.Add("@TableName", "Content");
            mysql.Parameters.Add("@SelectField", "*");
            mysql.Parameters.Add("@SortString", "");
            mysql.Parameters.Add("@ArrangeField", "id");
            mysql.Parameters.Add("@PageSize", 5);
            mysql.Parameters.Add("@ThisPage", thisPage);
            mysql.Parameters.Add("@SearchCondition", "");

            mysql.Parameters.Add("@SearchType", 1);
            // 获取总行数。
            var allRowCount = Convert.ToInt32(mysql.SPExecuteScalar("sp_Pagination"));

            mysql.Parameters["@SearchType"] = 0;

            // 获取分页后的数据表。
            var table = mysql.SPGetDataTable("sp_Pagination");

            var pager = new PagerControl(context, allRowCount, 5, thisPage)
            {
                PagerTemplate = "pagingTmpl.json"
            };

            var Grid = new DataGridControl();
            Grid.Template = "Account/list_.html";

            //添加数据网格控件到主控件。
            Grid.DataSource = table;
            Main.ControlTags.Add("List", Grid);

            //添加数据网格分页显示控件到主控件。
            Main.ControlTags.Add("PageCode", pager);

            Main.Template = "Account/list.html";

            IncludeExecutionTime = true;
            await Task.CompletedTask;
        }
    }
}