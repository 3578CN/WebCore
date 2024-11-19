using System.Text.RegularExpressions;

namespace CBC.WebCore.Common.StringHelpers
{
    /// <summary>
    /// 正则表达式处理类。
    /// </summary>
    public class RegexHelper
    {
        /// <summary>
        /// 不区分大小写的单行模式字符串替换。
        /// </summary>
        /// <param name="str">要查找的字符串。</param>
        /// <param name="regexStr">正则表达式。</param>
        /// <param name="matchStr">要替换的字符串。</param>
        /// <param name="isRightToLeft">是否从右向左进行搜索。</param>
        /// <returns>替换后的字符串。</returns>
        public static string Replace(string str, string regexStr, string matchStr, bool isRightToLeft = false)
        {
            return Regex.Replace(str, regexStr, matchStr, RegexOptions.IgnoreCase | RegexOptions.Singleline | (isRightToLeft ? RegexOptions.RightToLeft : RegexOptions.None) | RegexOptions.Compiled);
        }

        /// <summary>
        /// 不区分大小写的单行模式字符串分割。
        /// </summary>
        /// <param name="str">要分割的字符串。</param>
        /// <param name="regexStr">正则表达式。</param>
        /// <param name="isRightToLeft">是否从右向左进行搜索。</param>
        /// <returns>分割后的字符串数组。</returns>
        public static string[] Split(string str, string regexStr, bool isRightToLeft = false)
        {
            return Regex.Split(str, regexStr, RegexOptions.IgnoreCase | RegexOptions.Singleline | (isRightToLeft ? RegexOptions.RightToLeft : RegexOptions.None) | RegexOptions.Compiled);
        }
    }
}
