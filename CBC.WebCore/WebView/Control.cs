using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.Common.StringHelpers;

namespace CBC.WebCore.WebView
{
    /// <summary>
    /// 动态页面控件类。
    /// </summary>
    public class Control : IControl
    {
        private readonly IDictionary<string, object> _dataTags = new Dictionary<string, object>();
        private readonly IDictionary<string, IControl> _controlTags = new Dictionary<string, IControl>();
        private string _template;
        private StringBuilder _controlHtml = new();
        private bool _clearTags = true;

        #region 属性

        /// <summary>
        /// 动态页面数据标签。
        /// </summary>
        public IDictionary<string, object> DataTags => _dataTags;

        /// <summary>
        /// 动态页面控件标签。
        /// </summary>
        public IDictionary<string, IControl> ControlTags => _controlTags;

        /// <summary>
        /// 设置模板文件路径。
        /// </summary>
        public string Template
        {
            set => _template = value;
        }

        /// <summary>
        /// 获取或设置控件 HTML 代码。
        /// </summary>
        public string ControlHtml
        {
            get => _controlHtml.ToString();
            set => _controlHtml = new StringBuilder(value);
        }

        /// <summary>
        /// 设置是否清理模板标签（默认清理）。
        /// </summary>
        public bool ClearTags
        {
            set => _clearTags = value;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 异步处理模板代码。
        /// </summary>
        /// <returns>异步操作，表示模板代码处理的完成。</returns>
        protected virtual async Task ProcessTemplateCodeAsync()
        {
            if (!string.IsNullOrEmpty(_template))
            {
                _controlHtml = await ResourceProcessor.LoadTemplateAsync(_template);
            }
        }

        /// <summary>
        /// 替换动态页面数据标签。
        /// </summary>
        private void ReplaceDataTags()
        {
            foreach (var tag in _dataTags)
            {
                if (tag.Value != null)
                {
                    _controlHtml.ReplaceIgnoreCase($"{{$D:{tag.Key}}}", tag.Value.ToString());
                }
            }
        }

        /// <summary>
        /// 异步替换动态页面控件标签。
        /// </summary>
        private async Task ReplaceControlTagsAsync()
        {
            foreach (var tag in _controlTags)
            {
                _controlHtml.ReplaceIgnoreCase($"{{$C:{tag.Key}}}", await tag.Value.GetControlHtmlAsync());
            }
        }

        /// <summary>
        /// 异步获取动态页面控件的 HTML 代码。
        /// </summary>
        /// <returns>返回包含动态页面控件 HTML 代码的异步任务。</returns>
        public async Task<string> GetControlHtmlAsync()
        {
            // 异步加载模板代码。
            await ProcessTemplateCodeAsync();

            // 替换动态页面数据标签。
            ReplaceDataTags();

            // 异步替换动态页面控件标签。
            await ReplaceControlTagsAsync();

            var controlHtml = _controlHtml.ToString();

            // 是否清理标签。
            if (_clearTags)
            {
                controlHtml = RegexHelper.Replace(controlHtml, @"{\$[a-zA-Z0-9:-_]+}", "");
            }

            return controlHtml;
        }

        #endregion
    }
}