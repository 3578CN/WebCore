using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CBC.WebCore.Common.StringHelpers
{
    /// <summary>
    /// 字符串处理类。
    /// </summary>
    public static class StringHelper
    {
        #region 计算字符串的长度

        /// <summary>
        /// 计算字符串的长度。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <returns>返回长度。</returns>
        public static int StringLength(string str)
        {
            return Encoding.UTF8.GetByteCount(str);
        }

        #endregion

        #region 生成随机字符串

        /// <summary>
        /// 生成随机字符串。
        /// </summary>
        /// <param name="constant">要生成的随机数字符数组。</param>
        /// <param name="length">长度。</param>
        /// <returns>返回随机字符串。</returns>
        public static string RandString(char[] constant, int length)
        {
            StringBuilder newRandom = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[8];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    int rnd = BitConverter.ToInt32(buffer, 0);
                    newRandom.Append(constant[Math.Abs(rnd % constant.Length)]);
                }
            }
            return newRandom.ToString();
        }

        #endregion

        #region 转化编码

        /// <summary>
        /// 转换编码 UTF8 到 GB2312。
        /// </summary>
        /// <param name="src">字符串。</param>
        /// <returns>返回转换后的字符串。</returns>
        public static string UTF8_GB2312(string src)
        {
            if (string.IsNullOrEmpty(src))
                return "";
            Encoding utf8 = Encoding.UTF8;
            Encoding gb2312 = Encoding.GetEncoding("gb2312");
            byte[] utfBytes = utf8.GetBytes(src);
            byte[] gbBytes = Encoding.Convert(utf8, gb2312, utfBytes);
            return gb2312.GetString(gbBytes);
        }

        /// <summary>
        /// 转换编码 GB2312 到 UTF8。
        /// </summary>
        /// <param name="src">字符串。</param>
        /// <returns>返回转换后的字符串。</returns>
        public static string GB2312_UTF8(string src)
        {
            if (string.IsNullOrEmpty(src))
                return "";
            Encoding gb2312 = Encoding.GetEncoding("gb2312");
            Encoding utf8 = Encoding.UTF8;
            byte[] gbBytes = gb2312.GetBytes(src);
            byte[] utfBytes = Encoding.Convert(gb2312, utf8, gbBytes);
            return utf8.GetString(utfBytes);
        }

        #endregion

        #region 字符截取

        /// <summary>
        /// 按字节截取字符串。
        /// </summary>
        /// <param name="sInString">源字符串。</param>
        /// <param name="iCutLength">截取长度。</param>
        /// <returns>返回截取的字符串。</returns>
        public static string CutStr(string sInString, int iCutLength)
        {
            if (string.IsNullOrEmpty(sInString) || iCutLength <= 0)
                return "";
            Encoding encoding = Encoding.GetEncoding("Shift_JIS");
            byte[] bytes = encoding.GetBytes(sInString);
            if (bytes.Length <= iCutLength)
                return sInString;
            return encoding.GetString(bytes, 0, iCutLength);
        }

        /// <summary>
        /// 截取中文字符。
        /// </summary>
        /// <param name="str_value">源字符串。</param>
        /// <param name="str_len">截取长度。</param>
        /// <returns>返回截取的字符串。</returns>
        public static string CutGBStr(string str_value, int str_len)
        {
            if (string.IsNullOrEmpty(str_value) || str_len <= 0)
                return str_value;
            byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(str_value);
            return Encoding.GetEncoding("GB2312").GetString(bytes, 0, Math.Min(bytes.Length, str_len));
        }

        #endregion

        #region 读取指定编码的流

        /// <summary>
        /// 读取GB2312编码的文件流。
        /// </summary>
        /// <param name="s">文件流。</param>
        /// <returns>返回一个字符串。</returns>
        public static string ReadGB2312Stream(Stream s)
        {
            using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("GB2312")))
            {
                return sr.ReadToEnd();
            }
        }

        #endregion

        #region 判断是否是数字

        /// <summary>
        /// 判断是否是数字。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return str.All(char.IsDigit);
        }

        #endregion

        #region 用 MD5 算法对字符进行加密

        /// <summary>
        /// 用 MD5 算法对 UTF8 格式字符串进行加密。
        /// </summary>
        /// <param name="str">需要加密的字符串。</param>
        /// <returns>加密后的字符串。</returns>
        public static string UTF8_MD5Encrypt(string str)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(str);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

        /// <summary>
        /// 用 MD5 算法对字符串进行加密。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <returns></returns>
        public static string MD5Encrypt(string str)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.Default.GetBytes(str);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        #endregion

        #region 不区分大小写的字符串替换

        /// <summary>
        /// 不区分大小写的字符串替换。
        /// </summary>
        /// <param name="original">源字符串。</param>
        /// <param name="pattern">要替换的字符串。</param>
        /// <param name="replacement">替换后的字符串。</param>
        /// <returns>返回替换结果。</returns>
        public static string Replace(string original, string pattern, string replacement)
        {
            if (string.IsNullOrEmpty(original))
                return "";
            return Regex.Replace(original, Regex.Escape(pattern), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 在 StringBuilder 中替换所有出现的字符串，忽略大小写。
        /// </summary>
        /// <param name="sb">要操作的 StringBuilder 对象。</param>
        /// <param name="oldValue">要替换的旧字符串。</param>
        /// <param name="newValue">用于替换的新字符串。</param>
        /// <returns>处理后的 StringBuilder 对象。</returns>
        public static StringBuilder ReplaceIgnoreCase(this StringBuilder sb, string oldValue, string newValue)
        {
            // 检查参数是否为空。
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (string.IsNullOrEmpty(oldValue)) throw new ArgumentException("要替换的字符串不能为空。", nameof(oldValue));

            // 将 StringBuilder 转换为字符串。
            string sbString = sb.ToString();

            // 通过 StringComparison.OrdinalIgnoreCase 实现不区分大小写的替换。
            sbString = sbString.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);

            // 清空 StringBuilder 并将替换后的字符串重新加入 StringBuilder。
            sb.Clear();
            sb.Append(sbString);

            return sb;
        }


        #endregion

        #region 全半角转换

        /// <summary>
        /// 将半角字符转换成全角字符。
        /// </summary>
        /// <param name="input">字符串。</param>
        /// <returns>返回转换后的字符串。</returns>
        public static string ToSBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                    c[i] = (char)12288;
                else if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 将全角字符转换成半角字符。
        /// </summary>
        /// <param name="input">字符串。</param>
        /// <returns>返回转换后的字符串。</returns>
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                    c[i] = (char)32;
                else if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        #endregion

        #region 将 unicode 字符转换为汉字

        /// <summary>
        /// 将 unicode 字符转换为汉字。
        /// </summary>
        /// <param name="unicodeString">字符串。</param>
        /// <returns>返回转换后的字符串。</returns>
        public static string ConvertUnicodeStringToChinese(string unicodeString)
        {
            if (string.IsNullOrEmpty(unicodeString))
                return string.Empty;
            return Regex.Replace(unicodeString, @"\\u([0-9a-fA-F]{4})", m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
        }

        #endregion

        #region 补充字符串到指定长度

        /// <summary>
        /// 补充字符串到指定长度。
        /// </summary>
        /// <param name="originalStr">原始字符串。</param>
        /// <param name="newsStr">补充字符。</param>
        /// <param name="length">总长度。</param>
        /// <returns></returns>
        public static string SupplyStr(string originalStr, string newsStr, int length)
        {
            int originalCount = Encoding.GetEncoding("GB2312").GetByteCount(originalStr);
            int newCount = Encoding.GetEncoding("GB2312").GetByteCount(newsStr);
            if (originalCount < length)
            {
                int m = (length - originalCount) / newCount;
                int n = (length - originalCount) % newCount;
                if (n == 1)
                {
                    originalStr += " ";
                }
                for (int i = 0; i < m; i++)
                {
                    originalStr += newsStr;
                }
            }
            return originalStr;
        }

        #endregion

        #region 获得两个字符串中间的字符串

        /// <summary>
        ///  获得两个字符串中间的字符串。
        /// </summary>
        /// <param name="original">原始字符串。</param>
        /// <param name="startStr">开始字符串。</param>
        /// <param name="endStr">结束字符串。</param>
        /// <returns>返回一个字符串。</returns>
        public static string GetMiddleStr(string original, string startStr, string endStr)
        {
            int s = original.IndexOf(startStr, StringComparison.Ordinal);
            int e = original.IndexOf(endStr, StringComparison.Ordinal);
            if (e > s)
            {
                return original.Substring(s + startStr.Length, e - s - startStr.Length);
            }
            return "";
        }

        #endregion

        #region 统计某个字符串中包含另外一个字符串的个数

        /// <summary>
        ///  统计某个字符串中包含另外一个字符串的个数。
        /// </summary>
        /// <param name="original">要统计的字符串。</param>
        /// <param name="include">包含的字符串。</param>
        /// <param name="ignoreCase">是否忽略大小写。</param>
        /// <returns></returns>
        public static int IncludeString(string original, string include, bool ignoreCase)
        {
            if (ignoreCase)
            {
                original = Replace(original, include, "");
            }
            else
            {
                original = original.Replace(include, "");
            }
            return (original.Length - original.Replace(include, "").Length) / include.Length;
        }

        #endregion

        #region 判断字符是否是 IPV4 地址

        /// <summary>
        /// 判断字符是否是 IPV4 地址。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <returns>返回一个布尔值。</returns>
        public static bool IsIP(string str)
        {
            return Regex.IsMatch(str, @"^(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$");
        }

        #endregion
    }
}