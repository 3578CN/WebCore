using System.Collections;
using System.Reflection;

namespace CBC.WebCore.WebSite.Parsers
{
    /// <summary>
    /// 标签解析器工厂类，负责动态创建和缓存标签解析器实例。通过缓存机制，减少实例创建的开销，提高系统性能，并确保标签解析器的高效管理。
    /// </summary>
    public class TagParserFactory
    {
        /// <summary>
        /// 标签解析器实例缓存。
        /// </summary>
        private static readonly Hashtable _tagParserCache = [];

        /// <summary>
        /// 获取标签解析器对象的实例。
        /// </summary>
        /// <param name="classFullName">类的完全限定名。</param>
        /// <returns>返回对应的标签解析器实例。</returns>
        public static IParser GetTagParserInstance(string classFullName)
        {
            if (_tagParserCache[classFullName] == null)
            {
                // 缓存中不存在该实例。
                try
                {
                    // 获取主程序程序集（引用了此类库的应用程序程序集）。
                    var entryAssembly = Assembly.GetEntryAssembly();

                    // 在主程序集内查找类型。
                    Type type = entryAssembly.GetType(classFullName, throwOnError: false, ignoreCase: false);

                    // 添加到缓存。
                    _tagParserCache[classFullName] = (IParser)Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format($"反射标签解析器类型“{classFullName}”时出现错误。\r\n{e.Message}"));
                }

                return _tagParserCache[classFullName] as IParser;
            }
            else
            {
                // 找到缓存实例，直接返回。
                return _tagParserCache[classFullName] as IParser;
            }
        }
    }
}