using System.Data;
using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.WebSite.Parsers.TagParsers;
using CBC.WebCore.WebSite.Parsers.TagsEntity;

namespace CBC.WebCore.WebSite.Parsers.ParserImpls
{
    /// <summary>
    /// 数据网格解析器，负责将页面中的标签解析为数据网格结构。
    /// 通常用于解析列表或列表页的数据，可以解析整个数据表或分页数据。
    /// </summary>
    public class DataGridParserImpl
    {
        private readonly DataTags _dataTags;
        private readonly ValueTagParser _valueTagParser;

        #region 构造函数

        /// <summary>
        /// 初始化 <see cref="DataGridParserImpl"/> 类的新实例。
        /// </summary>
        /// <param name="dataTags">数据标签属性集合。</param>
        /// <param name="formatValue">可选的格式化数据表内值的委托方法。</param>
        public DataGridParserImpl(DataTags dataTags, FormatValue formatValue = null)
        {
            _dataTags = dataTags; // 初始化数据标签属性集合。
            _valueTagParser = new ValueTagParser // 初始化值标签解析器，并设置格式化方法。
            {
                FormatValue = formatValue
            };
        }

        #endregion

        #region 解析方法

        /// <summary>
        /// 解析页面 HTML 代码中的数据表标签，并将数据表内容替换到页面代码中。
        /// </summary>
        /// <param name="pageCode">页面 HTML 代码。</param>
        /// <param name="table">数据表对象，包含要解析的数据。</param>
        public async Task ParseAsync(StringBuilder pageCode, DataTable table)
        {
            // 从资源处理器中加载模板代码。
            var templateCode = await ResourceProcessor.LoadTemplateAsync(_dataTags["template"]);

            // 用于存储解析后的列表代码。
            var listCode = new StringBuilder();

            // 获取模板中的值标签集合列表。
            var valueTagList = ValueTagParser.GetValueTags(templateCode.ToString());

            // 遍历数据表的每一行，解析数据并替换标签。
            foreach (DataRow row in table.Rows)
            {
                // 将模板代码克隆到新的 StringBuilder 中。
                var code = new StringBuilder(templateCode.ToString());

                // 遍历值标签，替换每一个标签的值。
                foreach (var valueTag in valueTagList)
                {
                    // 初始化一个空的值。
                    var value = string.Empty;
                    try
                    {
                        // 尝试获取数据表中当前标签列的值，并解析替换到代码中。
                        value = row[valueTag.ColumnName].ToString();
                        await _valueTagParser.ParseAsync(code, valueTag, value);
                    }
                    catch (Exception ex)
                    {
                        // 捕获异常并抛出带有详细信息的异常。
                        throw new Exception($"解析标签 {valueTag.ColumnName} 时发生错误: {ex.Message}");
                    }
                }

                // 将解析后的代码追加到列表代码中。
                listCode.Append(code);
            }

            // 将页面中的占位标签替换为解析后的列表代码。
            pageCode.Replace(_dataTags.TagCode, listCode.ToString());

            // 确保任务已完成。
            await Task.CompletedTask;
        }

        #endregion
    }
}