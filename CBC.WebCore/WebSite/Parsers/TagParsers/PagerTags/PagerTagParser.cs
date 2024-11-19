using System.Data;
using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.WebSite.Parsers.TagsEntity;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace CBC.WebCore.WebSite.Parsers.TagParsers.PagerTags
{
    /// <summary>
    /// 数据网格分页标签解析类。
    /// </summary>
    public class PagerTagParser
    {
        /// <summary>
        /// 分页代码模板文件。
        /// </summary>
        public string PagerTemplate { get; set; }

        private readonly DataTable _dataTable; // 数据表。
        private readonly int _totalRowCount; // 总行数。
        private readonly int _totalPageCount; // 总页数。
        private readonly int _pageSize; // 每页行数。
        private readonly int _currentPage; // 当前页码。
        private readonly string _pageUrl; // 页面 URL。

        #region 构造函数

        /// <summary>
        /// 普通分页构造函数。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <param name="totalRowCount">总行数。</param>
        /// <param name="dataTags">（标签中必须有 PageSize 参数）。</param>
        /// <param name="currentPage">当前页码。</param>
        public PagerTagParser(HttpContext context, int totalRowCount, DataTags dataTags, int currentPage)
            : this(context, null, dataTags, currentPage)
        {
            _totalRowCount = totalRowCount;
            _totalPageCount = CalculateTotalPageCount(_totalRowCount, _pageSize);
        }

        /// <summary>
        /// 数据表分页构造函数。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <param name="dataTable">数据表。</param>
        /// <param name="dataTags">数据标签属性集合（标签中必须有 PageSize 参数）。</param>
        /// <param name="currentPage">当前页码。</param>
        public PagerTagParser(HttpContext context, DataTable dataTable, DataTags dataTags, int currentPage)
        {
            _dataTable = dataTable;
            _pageSize = int.Parse(dataTags["pageSize"]);
            _currentPage = currentPage;
            var pageUrl = context.GetAbsolutePath();

            #region 从页面 URL 中去掉页码

            // 找到最后一个 '.' 的位置。
            int lastDotIndex = pageUrl.LastIndexOf('.');

            // 找到最后一个下划线 '_' 的位置，并确保它在最后的 '.' 之前。
            int lastUnderscoreIndex = pageUrl.LastIndexOf('_');

            // 检查下划线到点之间是否都是数字。
            if (lastUnderscoreIndex > 0 && lastUnderscoreIndex < lastDotIndex)
            {
                string numberPart = pageUrl.Substring(lastUnderscoreIndex + 1, lastDotIndex - lastUnderscoreIndex - 1);

                // 检查下划线后面是否是数字。
                if (int.TryParse(numberPart, out _))
                {
                    // 去掉下划线及后面的数字部分。
                    _pageUrl = string.Concat(pageUrl.AsSpan(0, lastUnderscoreIndex), pageUrl.AsSpan(lastDotIndex));
                }
            }

            #endregion

            if (_dataTable != null)
            {
                _totalRowCount = _dataTable.Rows.Count;
                _totalPageCount = CalculateTotalPageCount(_totalRowCount, _pageSize);
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 计算总页数的工具方法。
        /// </summary>
        /// <param name="totalRowCount">总行数。</param>
        /// <param name="pageSize">每页行数。</param>
        /// <returns>返回总页数。</returns>
        private static int CalculateTotalPageCount(int totalRowCount, int pageSize)
        {
            return (totalRowCount + pageSize - 1) / pageSize;
        }

        /// <summary>
        /// 返回分页后的数据表。
        /// </summary>
        /// <returns>返回一个分页后的数据表。</returns>
        public DataTable GetPagedDataTable()
        {
            if (_dataTable == null) return null;

            // 创建新的数据表，复制结构。
            var paginatedTable = _dataTable.Clone();

            // 计算起始行数和结束行数。
            var startRow = (_currentPage - 1) * _pageSize;
            var endRow = Math.Min(_currentPage * _pageSize, _totalRowCount);

            // 拷贝行数据到新的数据表。
            for (var i = startRow; i < endRow; i++)
            {
                paginatedTable.ImportRow(_dataTable.Rows[i]);
            }

            return paginatedTable;
        }

        #endregion

        #region 异步处理模板代码并返回分页代码

        /// <summary>
        /// 异步处理模板代码并返回分页代码。
        /// </summary>
        /// <returns>异步操作，表示分页代码的处理和生成已经完成。</returns>
        public async Task<StringBuilder> ProcessTemplateCodeAsync()
        {
            // 使用 LoadTemplateAsync 方法加载模板内容。
            var templateContent = await Common.ResourceProcessor.LoadTemplateAsync(PagerTemplate, true);

            // 将模板内容解析为 JObject 对象。
            var document = JObject.Parse(templateContent.ToString());

            // 获取 pagerTemplate 对象。
            var pagerTemplate = document["pagerTemplate"];

            // 获取模板的各部分。
            var templateCode = pagerTemplate["templateCode"]?.ToString();
            var pagerNav = pagerTemplate["pagerNav"]?.ToString();

            // 使用 ReplaceTag 方法替换分页信息的占位符。
            pagerNav = ReplaceTag(pagerNav, "TotalRowCount", _totalRowCount);
            pagerNav = ReplaceTag(pagerNav, "TotalPageCount", _totalPageCount);
            pagerNav = ReplaceTag(pagerNav, "PageSize", _pageSize);
            pagerNav = ReplaceTag(pagerNav, "CurrentPage", _currentPage);

            // 获取分页链接的 HTML。
            var firstPage = GetPageLink(pagerTemplate, _currentPage > 1, "valid_FirstPage", "invalid_FirstPage", 1);
            var lastPage = GetPageLink(pagerTemplate, _currentPage < _totalPageCount, "valid_LastPage", "invalid_LastPage", _totalPageCount);

            var previousPage = GetPageLink(pagerTemplate, _currentPage > 1, "valid_PreviousPage", "invalid_PreviousPage", _currentPage - 1);
            var nextPage = GetPageLink(pagerTemplate, _currentPage < _totalPageCount, "valid_NextPage", "invalid_NextPage", _currentPage + 1);

            var numberPaginate = GetNumberPaginate(pagerTemplate);
            var dropdownPaginate = GetDropdownPaginate(pagerTemplate);

            // 生成最终的分页代码，并替换相应的占位符。
            templateCode = ReplaceTag(templateCode, "PagerNav", pagerNav);
            templateCode = ReplaceTag(templateCode, "FirstPage", firstPage);
            templateCode = ReplaceTag(templateCode, "PreviousPage", previousPage);
            templateCode = ReplaceTag(templateCode, "NextPage", nextPage);
            templateCode = ReplaceTag(templateCode, "LastPage", lastPage);
            templateCode = ReplaceTag(templateCode, "NumberPaginate", numberPaginate);
            templateCode = ReplaceTag(templateCode, "DropdownPaginate", dropdownPaginate);

            // 将生成的 HTML 存储到 ControlHtml 变量中。
            return new StringBuilder(templateCode);
        }

        /// <summary>
        /// 根据页面状态返回页码链接。
        /// </summary>
        /// <param name="pagerTemplate">分页模板。</param>
        /// <param name="isValid">是否有效。</param>
        /// <param name="validKey">有效时的键名。</param>
        /// <param name="invalidKey">无效时的键名。</param>
        /// <param name="pageNumber">页码。</param>
        /// <returns>页码链接。</returns>
        private string GetPageLink(JToken pagerTemplate, bool isValid, string validKey, string invalidKey, int pageNumber)
        {
            // 根据有效性选择键名。
            var key = isValid ? validKey : invalidKey;

            // 从 pagerTemplate 获取对应键的值，确保值不为空。
            var pageLink = pagerTemplate[key]?.ToString();

            // 如果是有效状态，替换 "PageUrl" 的占位符。
            return isValid ? ReplaceTag(pageLink, "PageUrl", _pageUrl.Insert(_pageUrl.IndexOf('.'), $"_{pageNumber}")) : pageLink;

        }

        /// <summary>
        /// 生成数字分页代码。
        /// </summary>
        /// <param name="pagerTemplate">分页模板。</param>
        /// <returns>返回生成的数字分页代码。</returns>
        private string GetNumberPaginate(JToken pagerTemplate)
        {
            var numberPaginate = new StringBuilder();

            // 获取分页的起始页和结束页。
            var startPage = Math.Max(1, _currentPage - (pagerTemplate["prevPageCount"]?.ToObject<int>() ?? 0));
            var endPage = Math.Min(_totalPageCount, _currentPage + (pagerTemplate["nextPageCount"]?.ToObject<int>() ?? 0));

            for (var i = startPage; i <= endPage; i++)
            {
                string pageLink;

                if (i == _currentPage)
                {
                    // 当前页使用无效的分页链接模板。
                    pageLink = pagerTemplate["invalid_NumberPaginate"]?.ToString();
                }
                else
                {
                    // 其他页使用有效的分页链接模板，并替换 "PageUrl" 占位符。
                    pageLink = ReplaceTag(pagerTemplate["valid_NumberPaginate"]?.ToString(), "PageUrl", _pageUrl.Insert(_pageUrl.IndexOf('.'), $"_{i}"));
                }

                // 替换 "PageNumber" 占位符。
                numberPaginate.Append(ReplaceTag(pageLink, "PageNumber", i));
            }

            return numberPaginate.ToString();
        }

        /// <summary>
        /// 生成下拉列表分页代码。
        /// </summary>
        /// <param name="pagerTemplate">分页模板。</param>
        /// <returns>返回生成的下拉列表分页代码。</returns>
        private string GetDropdownPaginate(JToken pagerTemplate)
        {
            var dropdownPaginate = new StringBuilder();

            // 获取分页的起始页和结束页。
            var startPage = Math.Max(1, _currentPage - (pagerTemplate["prevPageCount"]?.ToObject<int>() ?? 0));
            var endPage = Math.Min(_totalPageCount, _currentPage + (pagerTemplate["nextPageCount"]?.ToObject<int>() ?? 0));

            for (var i = startPage; i <= endPage; i++)
            {
                string pageLink;

                if (i == _currentPage)
                {
                    // 当前页使用选中的下拉列表模板。
                    pageLink = pagerTemplate["selected_DropdownPaginate"]?.ToString();
                }
                else
                {
                    // 其他页使用未选中的下拉列表模板。
                    pageLink = ReplaceTag(pagerTemplate["unselected_DropdownPaginate"]?.ToString(), "PageUrl", _pageUrl.Insert(_pageUrl.IndexOf('.'), $"_{i}"));
                }

                // 替换 "PageNumber" 占位符。
                dropdownPaginate.Append(ReplaceTag(pageLink, "PageNumber", i));
            }

            return dropdownPaginate.ToString();
        }

        /// <summary>
        /// 替换标签 StringBuilder 扩展方法。
        /// </summary>
        /// <param name="tagCode">要替换的代码。</param>
        /// <param name="tagName">标签名称。</param>
        /// <param name="value">值。</param>
        /// <returns>替换后的代码。</returns>
        private static string ReplaceTag(string tagCode, string tagName, object value)
        {
            return tagCode.Replace($"${tagName}$", value.ToString());
        }

        #endregion
    }
}