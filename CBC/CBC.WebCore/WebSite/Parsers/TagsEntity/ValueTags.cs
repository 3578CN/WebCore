using System.Collections.Specialized;
using CBC.WebCore.Common.StringHelpers;

namespace CBC.WebCore.WebSite.Parsers.TagsEntity
{
    /// <summary>
    /// 数据表值标签属性集合。
    /// </summary>
    public class ValueTags : NameValueCollection
    {
        /// <summary>
        /// 列名称。
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// 数据表值标签代码。
        /// </summary>
        public string TagCode { get; private set; }

        /// <summary>
        /// 数据表值标签属性代码。
        /// </summary>
        public string TagAttributeCode { get; private set; }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="tagCode">数据表值标签代码。</param>
        public ValueTags(string tagCode)
        {
            //获得列名称。
            ColumnName = RegexHelper.Replace(tagCode, @"{\$(.*?)(?:\..*?format.*?\((.*?)\).*?)?}", "$1").Trim();

            //获得数据表值标签代码。
            TagCode = tagCode;

            //获得数据表值标签属性代码。
            TagAttributeCode = RegexHelper.Replace(tagCode, @"{\$(.*?)(?:\..*?format.*?\((.*?)\).*?)?}", "$2");

            if (TagAttributeCode.Contains(','))
            {
                string[] paras = TagAttributeCode.Split(',');
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