using System.Text;
using System.Text.RegularExpressions;
using CBC.WebCore.WebSite.Parsers.TagsEntity;

namespace CBC.WebCore.WebSite.Parsers.TagParsers
{
    /// <summary>
    /// 扩展格式化数据表值委托方法。
    /// </summary>
    /// <param name="valueTag">数据表值标签属性集合。</param>
    /// <param name="value">数据表值。</param>
    /// <returns>返回格式化后的数据表值。</returns>
    public delegate string FormatValue(ValueTags valueTag, string value);

    /// <summary>
    /// 数据表值标签解析方法类。
    /// </summary>
    public class ValueTagParser
    {
        private FormatValue _formatValue;

        /// <summary>
        /// 设置或获取扩展格式化数据表值委托方法。
        /// </summary>
        public FormatValue FormatValue
        {
            set => _formatValue = value;
        }

        /// <summary>
        /// 异步解析数据表值标签的方法。
        /// </summary>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        /// <param name="valueTag">数据表值标签属性集合。</param>
        /// <param name="value">数据表值。</param>
        public async Task ParseAsync(StringBuilder pageHtml, ValueTags valueTag, string value)
        {
            // 检查是否包含特定属性标签代码。
            if (string.IsNullOrEmpty(valueTag.TagAttributeCode))
            {
                pageHtml.Replace(valueTag.TagCode, value);
            }
            else
            {
                await ReplaceValueAsync(pageHtml, valueTag, value);
            }
        }

        /// <summary>
        /// 获取模板代码中的数据表值标签集合。
        /// </summary>
        /// <param name="templateCode">模板代码。</param>
        /// <returns>包含数据表值标签的集合。</returns>
        public static IList<ValueTags> GetValueTags(string templateCode)
        {
            if (templateCode == null)
            {
                throw new ArgumentNullException(nameof(templateCode), "模板代码不能为空。");
            }

            // 定义正则表达式匹配 {$...} 标签。
            Regex tagRegex = new Regex(@"{\$(.*?)}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            var matches = tagRegex.Matches(templateCode);

            var valueTags = new List<ValueTags>();

            foreach (Match var in matches)
            {
                var attributeTagMatch = Regex.Match(var.Value, @"{\$(.*?)\..*?Format.*?\((.*?)\).*?}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                if (attributeTagMatch.Success)
                {
                    //是包含属性的内值标签。
                    valueTags.Add(new ValueTags(var.Value));
                }
                else
                {
                    //是不包含属性的内值标签。
                    var simpleTagMatch = Regex.Match(var.Value, @"{\$((?![^}]*(?:\:))[^}]*?)}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                    if (simpleTagMatch.Success)
                    {
                        valueTags.Add(new ValueTags(var.Value));
                    }
                }
            }

            return valueTags;
        }

        /// <summary>
        /// 异步替换数据表值的方法，支持多种属性处理。
        /// </summary>
        /// <param name="pageHtml">页面 Html 代码。</param>
        /// <param name="valueTag">数据表值标签属性集合。</param>
        /// <param name="value">数据表值。</param>
        private async Task ReplaceValueAsync(StringBuilder pageHtml, ValueTags valueTag, string value)
        {
            // 处理长度限制。
            if (int.TryParse(valueTag["length"], out int length) && length <= value.Length)
            {
                value = value.Substring(0, length);
            }

            // 处理日期格式化。
            if (!string.IsNullOrEmpty(valueTag["dateTime"]) && DateTime.TryParse(value, out var dateValue))
            {
                value = dateValue.ToString(valueTag["dateTime"]);
            }

            // 处理隐藏 IP 地址。
            if (!string.IsNullOrEmpty(valueTag["hiddenIP"]))
            {
                value = HideIPAddress(value, valueTag["hiddenIP"]);
            }

            // 处理追加字符串。
            if (!string.IsNullOrEmpty(valueTag["addEndString"]))
            {
                value += valueTag["addEndString"];
            }

            // 使用委托进行扩展格式化。
            if (_formatValue != null)
            {
                value = _formatValue(valueTag, value);
            }

            // 替换页面 HTML 中的标签值。
            pageHtml.Replace(valueTag.TagCode, value);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 隐藏 IP 地址的部分信息，根据给定的掩码格式隐藏。
        /// </summary>
        /// <param name="ipAddress">要隐藏的 IP 地址。</param>
        /// <param name="mask">掩码格式。</param>
        /// <returns>返回隐藏后的 IP 地址。</returns>
        private string HideIPAddress(string ipAddress, string mask)
        {
            var ipParts = ipAddress.Split('.');
            var maskChars = mask.ToCharArray(0, 4);
            var hiddenIP = new StringBuilder();

            for (int i = 0; i < maskChars.Length; i++)
            {
                hiddenIP.Append(maskChars[i] == '*' ? "*." : ipParts[i] + ".");
            }

            // 移除最后一个多余的点号。
            return hiddenIP.ToString().TrimEnd('.');
        }
    }
}