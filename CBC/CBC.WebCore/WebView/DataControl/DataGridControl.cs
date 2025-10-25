using System.Data;
using System.Text;
using CBC.WebCore.WebSite.Parsers.TagParsers;

namespace CBC.WebCore.WebView.DataControl
{
    /// <summary>
    /// 数据网格控件类。
    /// </summary>
    public class DataGridControl : Control
    {
        private FormatValue _formatValue;
        private DataTable _dataSource;

        #region 属性

        /// <summary>
        /// 设置格式化数据表值委托方法。
        /// </summary>
        public FormatValue FormatValue
        {
            set => _formatValue = value;
        }

        /// <summary>
        /// 设置控件的数据源表格。
        /// </summary>
        public DataTable DataSource
        {
            set => _dataSource = value;
        }

        #endregion

        #region 获取数据网格的 HTML 代码

        /// <summary>
        /// 异步加载并生成数据网格代码。
        /// </summary>
        /// <returns>异步操作，表示数据网格代码的加载和生成已完成。</returns>
        protected override async Task ProcessTemplateCodeAsync()
        {
            await base.ProcessTemplateCodeAsync();

            var htmlContent = new StringBuilder();
            var valueTagParser = new ValueTagParser
            {
                FormatValue = _formatValue
            };

            // 获取 HTML 模板中的值标签属性集合。
            var valueTagList = ValueTagParser.GetValueTags(ControlHtml);

            foreach (DataRow dataRow in _dataSource.Rows)
            {
                var processedHtml = new StringBuilder(ControlHtml);

                foreach (var valueTag in valueTagList)
                {
                    // 尝试替换数据表中的值标签。
                    try
                    {
                        var value = dataRow[valueTag.ColumnName]?.ToString() ?? string.Empty;
                        await valueTagParser.ParseAsync(processedHtml, valueTag, value);
                    }
                    catch (Exception ex)
                    {
                        // 如果发生异常，抛出新的异常，并保留原始异常的信息。
                        throw new Exception("在解析值标签时发生错误：", ex);
                    }
                }
                htmlContent.Append(processedHtml); // 将处理后的 PageHtml 追加到 htmlContent 中。
            }
            ControlHtml = htmlContent.ToString(); // 将累积的 htmlContent 内容赋值给 PageHtml 属性。
        }

        #endregion
    }
}