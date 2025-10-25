using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.WebSite.Parsers;
using CBC.WebCore.WebSite.Parsers.ParserImpls;
using CBC.WebCore.WebSite.Parsers.TagsEntity;

namespace CBC.TestApp.Handlers
{
    public class Index_Parser : IParser
    {
        public async Task ParseAsync(HttpContext context, StringBuilder pageHtml, DataTags dataTags)
        {
            var mysql = new WebCore.Common.DataHelpers.MySqlHelper(AppSettings.Current.ConnectionStrings["AppTestDB"]);
            var table = mysql.GetDataTable("SELECT * FROM Content where id=5");

            var page = new PageParserImpl(dataTags);
            await page.ParseAsync(context, pageHtml, dataTags, table);
        }
    }
}