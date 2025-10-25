using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.WebSite;
using CBC.WebCore.WebSite.Parsers;
using CBC.WebCore.WebSite.Parsers.ParserImpls;
using CBC.WebCore.WebSite.Parsers.TagParsers.PagerTags;
using CBC.WebCore.WebSite.Parsers.TagsEntity;

namespace CBC.TestApp.Handlers
{
    public class List_Parser : IParser
    {
        public async Task ParseAsync(HttpContext context, StringBuilder pageHtml, DataTags dataTags)
        {
            //当前页码。
            var thisPage = int.Parse(PageConfig.GetCurrent(context).Values["page"]);

            var mysql = new CBC.WebCore.Common.DataHelpers.MySqlHelper(AppSettings.Current.ConnectionStrings["AppTestDB"]);

            mysql.Parameters.Add("@TableName", "Content");
            mysql.Parameters.Add("@SelectField", "*");
            mysql.Parameters.Add("@SortString", "");
            mysql.Parameters.Add("@ArrangeField", "id");
            mysql.Parameters.Add("@PageSize", dataTags["pageSize"]);
            mysql.Parameters.Add("@ThisPage", thisPage);
            mysql.Parameters.Add("@SearchCondition", "");

            // 获取总行数。
            mysql.Parameters.Add("@SearchType", 1);
            var allRowCount = Convert.ToInt32(mysql.SPExecuteScalar("sp_Pagination"));

            //获取分页后的数据表。
            mysql.Parameters["@SearchType"] = 0;
            var table = mysql.SPGetDataTable("sp_Pagination");

            //分页导航标签解析方法。
            var pagination = new PagerTagParser(context, allRowCount, dataTags, thisPage)
            {
                PagerTemplate = PageConfig.GetCurrent(context).PagerTemplate
            };

            //分页导航解析。
            pageHtml.Replace("{$tag:PagerNav}", (await pagination.ProcessTemplateCodeAsync()).ToString());


            var page = new DataGridParserImpl(dataTags);
            await page.ParseAsync(pageHtml, table);
        }
    }
}