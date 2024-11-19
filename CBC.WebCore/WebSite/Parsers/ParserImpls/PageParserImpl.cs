using System.Data;
using System.Text;
using CBC.WebCore.WebSite.Parsers.TagParsers;
using CBC.WebCore.WebSite.Parsers.TagParsers.PagerTags;
using CBC.WebCore.WebSite.Parsers.TagsEntity;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite.Parsers.ParserImpls
{
    /// <summary>
    /// 页面解析器，负责解析页面中的数据标签（包括自定义标签 {$tag:...}）。
    /// 通常用于首页和内容页的数据处理，仅解析数据表格的首行内容。
    /// </summary>
    public class PageParserImpl
    {
        private readonly ValueTagParser _valueTagParser;
        private bool _isHiddenDataTag = true;
        private readonly IDictionary<string, object> _customTag = new Dictionary<string, object>();

        #region 构造函数

        /// <summary>
        /// 初始化 <see cref="PageParserImpl"/> 类的新实例。
        /// </summary>
        /// <param name="dataTag">数据标签属性集合。</param>
        /// <param name="formatValue">扩展格式化数据表内值的委托方法，可选。</param>
        public PageParserImpl(DataTags dataTag, FormatValue formatValue = null)
        {
            _valueTagParser = new ValueTagParser
            {
                FormatValue = formatValue
            };
        }

        #endregion

        #region 属性

        /// <summary>
        /// 设置或获取一个值，指示是否隐藏数据标签。默认值为 true，即隐藏数据标签。
        /// </summary>
        public bool IsHiddenDataTag
        {
            set => _isHiddenDataTag = value;
        }

        /// <summary>
        /// 获取分页显示的列名称。默认值为空，如果为空则不进行分页。
        /// </summary>
        public string PagerColumn { get; private set; }

        /// <summary>
        /// 获取自定义值标签集合。
        /// </summary>
        public IDictionary<string, object> CustomTag => _customTag;

        #endregion

        #region 页面标签解析方法

        /// <summary>
        /// 异步解析页面中的数据标签，并将结果替换到页面 HTML 代码中。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        /// <param name="dataTags">数据标签属性集合。</param>
        /// <param name="table">数据表对象。</param>
        public async Task ParseAsync(HttpContext context, StringBuilder pageHtml, DataTags dataTags, DataTable table)
        {
            // 检查是否隐藏数据标签。
            if (_isHiddenDataTag)
            {
                pageHtml.Replace(dataTags.TagCode, string.Empty);
            }

            // 解析数据表中的值标签。
            await ParseTableValuesAsync(context, pageHtml, table);

            // 解析自定义标签。
            await ParseCustomTagAsync(pageHtml);
        }

        /// <summary>
        /// 异步解析页面中的数据标签，并将结果替换到页面 HTML 代码中。
        /// </summary>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        /// <param name="dataTags">数据标签属性集合。</param>
        public async Task ParseAsync(StringBuilder pageHtml, DataTags dataTags)
        {
            // 检查是否隐藏数据标签。
            if (_isHiddenDataTag)
            {
                pageHtml.Replace(dataTags.TagCode, string.Empty);
            }

            // 解析自定义标签。
            await ParseCustomTagAsync(pageHtml);
        }

        /// <summary>
        /// 异步解析页面中的数据标签，并用指定的字符串值替换。
        /// </summary>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        /// <param name="dataTags">数据标签属性集合。</param>
        /// <param name="value">要替换的数据值。</param>
        public static async Task ParseAsync(StringBuilder pageHtml, DataTags dataTags, string value)
        {
            pageHtml.Replace(dataTags.TagCode, value);
            await Task.CompletedTask;
        }

        #endregion

        #region 私有方法

        // 异步解析数据表中第一行数据的值标签，并将结果替换到页面 HTML 代码中。
        private async Task ParseTableValuesAsync(HttpContext context, StringBuilder pageHtml, DataTable table)
        {
            var valueTagList = ValueTagParser.GetValueTags(pageHtml.ToString());
            var row0 = table.Rows[0];

            foreach (var valueTag in valueTagList)
            {
                try
                {
                    var value = row0[valueTag.ColumnName].ToString();

                    // 检查是否需要分页显示。
                    if (!string.IsNullOrEmpty(PagerColumn) && PagerColumn == valueTag.ColumnName)
                    {
                        var pagination = new ColValPagerTagParser(value);
                        value = pagination.GetColumnValue(context);
                        pageHtml.Replace("{$tag:PagerNav}", pagination.GetPagerNavCode());
                    }

                    await _valueTagParser.ParseAsync(pageHtml, valueTag, value);
                }
                catch (Exception ex)
                {
                    throw new Exception($"在解析标签 '{valueTag.ColumnName}' 时发生错误：{ex.Message}", ex);
                }
            }
        }

        // 异步解析自定义值标签，并将结果替换到页面 HTML 代码中。
        private async Task ParseCustomTagAsync(StringBuilder pageHtml)
        {
            foreach (var ct in _customTag)
            {
                if (ct.Value != null)
                {
                    pageHtml.Replace($"{{$Tag:{ct.Key}}}", ct.Value.ToString());
                }
            }
            await Task.CompletedTask;
        }

        #endregion
    }
}