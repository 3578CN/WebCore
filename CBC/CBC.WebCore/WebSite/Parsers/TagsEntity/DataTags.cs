using System.Collections.Specialized;
using CBC.WebCore.Common.StringHelpers;

namespace CBC.WebCore.WebSite.Parsers.TagsEntity
{
    /// <summary>
    /// 数据标签属性集合。
    /// </summary>
    public class DataTags : NameValueCollection
    {
        /// <summary>
        /// 数据标签名称。
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// 数据标签代码。
        /// </summary>
        public string TagCode { get; private set; }

        /// <summary>
        /// 数据标签属性代码。
        /// </summary>
        public string TagAttributeCode { get; private set; }

        /// <summary>
        /// 初始化 DataTags 类的新实例。通过解析传入的标签代码，提取数据标签的名称和属性。
        /// </summary>
        /// <param name="tagCode">标签代码。</param>
        public DataTags(string tagCode)
        {
            // 获取数据标签名称。
            TagName = RegexHelper.Replace(tagCode, @"{\$data.*?\((.*?)\).*?\:(.*?)}", "$1").Trim();

            // 获取数据标签代码。
            TagCode = tagCode;

            //数据标签属性代码。
            TagAttributeCode = RegexHelper.Replace(tagCode, @"{\$data.*?\((.*?)\).*?\:(.*?)}", "$2");

            if (TagAttributeCode.Contains(';'))
            {
                string[] paras = TagAttributeCode.Split(';');
                foreach (string para in paras)
                {
                    if (para.Contains('='))
                    {
                        string[] info = para.Split('=');
                        this[info[0].Trim()] = info[1].Trim();
                    }
                }
            }
            else if (TagAttributeCode.Contains('='))
            {
                string[] info = TagAttributeCode.Split('=');
                this[info[0].Trim()] = info[1].Trim();
            }
        }
    }
}