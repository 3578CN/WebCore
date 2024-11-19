using System.Text;
using System.Text.RegularExpressions;
using CBC.WebCore.Common.StringHelpers;
using CBC.WebCore.WebSite.Parsers.TagsEntity;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite.Parsers.TagParsers
{
    /// <summary>
    /// 数据标签解析类。
    /// </summary>
    public class DataTagParser
    {
        /// <summary>
        /// 异步数据标签解析方法。解析页面中的数据标签并处理相应逻辑。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        public static async Task ParseAsync(HttpContext context, StringBuilder pageHtml)
        {
            // 使用正则表达式匹配数据标签。匹配格式为：{$data(标签类型):(内容)}。
            var regex = new Regex(@"{\$data.*?\((.*?)\).*?\:(.*?)}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            var matches = regex.Matches(pageHtml.ToString());

            foreach (Match match in matches)
            {
                // 提取标签类型。
                string tagType = RegexHelper.Replace(match.Value, @"{\$data.*?\((.*?)\).*?\:(.*?)}", "$1").Trim();

                // 获取相应的标签解析器实例。
                var tagParser = TagParserFactory.GetTagParserInstance(tagType);

                // 如果未找到对应的标签解析器，跳过当前循环，避免进入死循环。
                if (tagParser == null) continue;

                // 创建数据标签对象。
                var dataTag = new DataTags(match.Value);

                // 异步解析当前标签。
                await tagParser.ParseAsync(context, pageHtml, dataTag);
            }
        }
    }
}