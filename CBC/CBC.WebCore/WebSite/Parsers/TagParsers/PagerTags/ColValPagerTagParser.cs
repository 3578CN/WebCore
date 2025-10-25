using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.Common.StringHelpers;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite.Parsers.TagParsers.PagerTags
{
    /// <summary>
    /// 数据表列值分页标签解析类。
    /// </summary>
    public class ColValPagerTagParser
    {
        private readonly string _columnValue;
        private bool _hasPageBreak;
        private int _currentPage;
        private string _pageUrl;
        private int _totalPageCount;
        private int _pageIndexLength;

        /// <summary>
        /// 初始化 ColValPagingTagParser 类的新实例。
        /// </summary>
        /// <param name="columnValue">列值。</param>
        public ColValPagerTagParser(string columnValue)
        {
            _columnValue = columnValue;
        }

        #region 返回分页后的列值

        /// <summary>
        /// 获取分页后的列值。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回当前页的列值，如果没有分页则返回完整列值。</returns>
        public string GetColumnValue(HttpContext context)
        {
            // 使用正则表达式匹配分页符并分割列值。
            var values = RegexHelper.Split(_columnValue, "<div style=\"page-break-after: always\">.*?</div>");

            // 计算总页数。
            _totalPageCount = values.Length;

            if (_totalPageCount > 1)
            {
                // 存在分页符。
                _hasPageBreak = true;

                // 获取当前请求的页面 URL。
                _pageUrl = context.GetAbsolutePath();

                // 计算页面 URL 中当前页码的字符长度。
                _pageIndexLength = (_pageUrl.LastIndexOf('.') - 1) - _pageUrl.LastIndexOf('_');

                // 解析当前页码。
                _currentPage = int.Parse(_pageUrl.Substring(_pageUrl.LastIndexOf('_') + 1, _pageIndexLength));

                // 返回当前页的列值。
                return values[_currentPage - 1];
            }

            // 如果没有分页符，返回完整的列值。
            return _columnValue;
        }

        #endregion

        #region 返回分页代码

        /// <summary>
        /// 获取分页导航的 HTML 代码。
        /// </summary>
        /// <returns>返回分页导航的 HTML 代码，如果没有分页则返回空字符串。</returns>
        public string GetPagerNavCode()
        {
            if (_hasPageBreak)
            {
                // 移除 URL 中的当前页码部分以生成基础 URL。
                var baseUrl = _pageUrl.Remove(_pageUrl.LastIndexOf('_') + 1, _pageIndexLength);

                var code = new StringBuilder();
                for (var i = 1; i <= _totalPageCount; i++)
                {
                    // 插入页码到基础 URL 中生成完整的分页 URL。
                    var url = baseUrl.Insert(baseUrl.IndexOf('.'), i.ToString());

                    // 根据当前页码生成对应的 HTML 代码。
                    code.AppendFormat("{0}{1}", (i == _currentPage) ? $"<b>{i}</b>" : $"<a href=\"{url}\">[{i}]</a>", (i != _totalPageCount) ? "&nbsp;" : "");
                }

                // 返回生成的分页导航代码。
                return code.ToString();
            }

            // 如果没有分页，则返回空字符串。
            return string.Empty;
        }

        #endregion
    }
}