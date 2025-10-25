using System.Text;
using CBC.WebCore.Common.FileHelpers;
using CBC.WebCore.Common.HttpHelpers;

namespace CBC.WebCore.Common
{
    /// <summary>
    /// 资源处理器，用于加载和处理与网站和用户界面相关的资源文件。
    /// 该类支持异步加载文件内容，未来可以扩展为处理更多类型的资源。
    /// </summary>
    public class ResourceProcessor
    {
        /// <summary>
        /// 异步加载指定路径的模板文件内容。
        /// </summary>
        /// <param name="template">模板文件的相对路径。</param>
        /// <param name="isPagerTemplate">是否是分页模板。</param>
        /// <returns>异步操作，返回包含模板内容的 <see cref="StringBuilder"/> 对象。</returns>
        /// <exception cref="ArgumentException">当模板文件路径为空时抛出此异常。</exception>
        /// <exception cref="FileNotFoundException">当模板文件未找到时抛出此异常。</exception>
        public static async Task<StringBuilder> LoadTemplateAsync(string template, bool isPagerTemplate = false)
        {
            if (string.IsNullOrEmpty(template))
            {
                throw new ArgumentException("模板文件路径不能为空。");
            }

            var templatePath = !isPagerTemplate ? AppSettings.Current.TemplatePath : AppSettings.Current.PagerTemplatePath;

            var filePath = HttpHelper.MapPath($"{templatePath}{template}");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"未找到模板文件：{filePath}");
            }

            return await FileHelper.ReadFileAsync(filePath);
        }
    }
}