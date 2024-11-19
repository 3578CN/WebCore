using CBC.WebCore.WebSite.Parsers;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// 页面处理程序配置类。
    /// </summary>
    public class PageConfig
    {
        internal const string Page_Config_Key = "Page_Config";
        private bool _useStringTemplateOnly = false;
        private string _handlerType = "CBC.WebCore.WebSite.Parsers.DefaultPageHandler";
        private IDictionary<string, string> _values = new Dictionary<string, string>();

        #region 属性

        /// <summary>
        /// 设置或获取模板文件。
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// 是否仅使用字符串模板。
        /// </summary>
        public bool UseStringTemplateOnly
        {
            get => _useStringTemplateOnly;
            set => _useStringTemplateOnly = value;
        }

        /// <summary>
        /// 翻页代码模板文件。
        /// </summary>
        public string PagerTemplate { get; set; }

        /// <summary>
        /// 设置或获取模板标签解析处理程序类的完全限定名。
        /// </summary>
        public string HandlerType
        {
            get => _handlerType;
            set => _handlerType = value;
        }

        /// <summary>
        /// 页面缓存时间。
        /// </summary>
        public int CacheDuration { get; set; }

        /// <summary>
        /// 页面保存方法。
        /// </summary>
        public SaveMethod SaveMethod { get; set; }

        /// <summary>
        /// URL 属性集合。
        /// </summary>
        public IDictionary<string, string> Values
        {
            get => _values;
            set => _values = value;
        }

        /// <summary>
        /// 索引器，用户检索 URL 属性集合。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                if (_values.TryGetValue(key, out string _string))
                {
                    return _string;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        /// <summary>
        /// 获取当前请求的页面处理程序配置。
        /// </summary>
        /// <param name="context">当前的 HTTP 上下文。</param>
        /// <returns>返回当前请求的页面处理程序配置。</returns>
        public static PageConfig GetCurrent(HttpContext context)
        {
            if (context.Items[Page_Config_Key] == null)
            {
                context.Items.Add(Page_Config_Key, UrlMapping.GetCurrent(context).PageConfig);
            }
            return context.Items[Page_Config_Key] as PageConfig;
        }

        #region 获取模板标签解析处理程序类型

        /// <summary>
        /// 获取模板标签解析处理程序类型。
        /// </summary>
        /// <returns></returns>
        public Parser GetTagParserType()
        {
            return PageFactory.CreateParserInstance(HandlerType);
        }

        #endregion
    }
}