using System.Text;
using System.Text.RegularExpressions;

namespace CBC.WebCore.WebSite.Parsers.TagParsers
{
    /// <summary>
    /// 加载页面标签解析类。用于解析和处理页面中的 {$load} 标签。
    /// </summary>
    public class LoadTagParser
    {
        /// <summary>
        /// 异步加载页面标签解析方法。支持远程页面请求和本地模板加载。
        /// </summary>
        /// <param name="pageHtml">页面 HTML 代码。</param>
        public static async Task ParseAsync(StringBuilder pageHtml)
        {
            // 定义用于匹配 {$load} 标签的正则表达式。
            var loadTagRegex = new Regex(@"{\$load.*?\:(.*?)}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            // 查找所有匹配的加载标签。
            var matches = loadTagRegex.Matches(pageHtml.ToString());

            // 创建 HttpClient 实例，用于处理远程页面请求。
            using var httpClient = new HttpClient();

            // 遍历所有匹配的标签并逐个处理。
            foreach (Match match in matches)
            {
                string value = match.Groups[1].Value.Trim(); // 提取并清理标签内容。
                string loadTagContent; // 用于存储加载的内容。

                // 判断是否为远程页面请求，通过检测是否为 HTTP(S) URL。
                if (IsRemoteUrl(value))
                {
                    loadTagContent = await LoadRemoteContentAsync(httpClient, value);
                }
                else
                {
                    // 如果是本地模板，调用 LoadTemplateAsync 方法加载模板内容。
                    loadTagContent = await LoadLocalTemplateAsync(value);
                }

                // 使用加载的内容替换原标签内容。
                pageHtml.Replace(match.Value, loadTagContent);
            }
        }

        /// <summary>
        /// 判断给定的字符串是否为远程 URL。
        /// </summary>
        /// <param name="url">要检查的字符串。</param>
        /// <returns>如果是远程 URL，返回 true；否则返回 false。</returns>
        private static bool IsRemoteUrl(string url)
        {
            return Regex.IsMatch(url, @"http(?:s)?\://.*?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        }

        /// <summary>
        /// 异步加载远程页面内容。
        /// </summary>
        /// <param name="httpClient">HttpClient 实例。</param>
        /// <param name="url">远程页面的 URL。</param>
        /// <returns>返回远程页面内容。</returns>
        private static async Task<string> LoadRemoteContentAsync(HttpClient httpClient, string url)
        {
            // 判断是否指定了编码格式。
            var encodingMatch = Regex.Match(url, @"(.*?)\s(.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            if (encodingMatch.Success)
            {
                // 如果指定了编码格式，将编码信息加入到请求头中。
                httpClient.DefaultRequestHeaders.AcceptCharset.TryParseAdd(encodingMatch.Groups[2].Value);
                url = encodingMatch.Groups[1].Value.Trim(); // 提取实际的 URL。
            }

            // 异步发送 HTTP 请求并获取页面内容。
            return await httpClient.GetStringAsync(url);
        }

        /// <summary>
        /// 异步加载本地模板内容。
        /// </summary>
        /// <param name="templatePath">本地模板的路径。</param>
        /// <returns>返回模板内容。</returns>
        private static async Task<string> LoadLocalTemplateAsync(string templatePath)
        {
            // 调用公共方法异步加载本地模板。
            return (await Common.ResourceProcessor.LoadTemplateAsync(templatePath)).ToString();
        }
    }
}