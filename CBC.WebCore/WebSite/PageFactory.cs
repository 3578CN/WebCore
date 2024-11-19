using System.Reflection;
using CBC.WebCore.WebSite.Parsers;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// 页面工厂类，负责创建 Parser 实例。
    /// </summary>
    public class PageFactory
    {
        /// <summary>
        /// 根据指定的处理程序类型创建 Parser 实例。
        /// </summary>
        /// <param name="handlerType">处理程序类型的完全限定名。</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public static Parser CreateParserInstance(string handlerType)
        {
            try
            {
                // 在当前程序集（即主程序的程序集）内查找类型。
                var entryAssembly = Assembly.GetEntryAssembly();
                Type type = entryAssembly?.GetType(handlerType, throwOnError: false, ignoreCase: false);

                if (type == null)
                {
                    // 如果在主程序集中找不到类型，则在指定的程序集 "CBC.WebCore" 中查找。
                    var targetAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "CBC.WebCore");

                    if (targetAssembly != null)
                    {
                        type = targetAssembly.GetType(handlerType, throwOnError: false, ignoreCase: false);
                    }
                }

                if (type == null)
                {
                    // 处理类型查找失败的情况。
                    throw new InvalidOperationException($"无法在当前程序集或 CBC.WebCore 程序集中找到类型 {handlerType}。");
                }

                // 创建实例并返回。
                return Activator.CreateInstance(type) as Parser;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format($"反射模板标签解析处理程序类型“{handlerType}”时出现错误。\r\n{ex.Message}"));
            }
        }
    }
}