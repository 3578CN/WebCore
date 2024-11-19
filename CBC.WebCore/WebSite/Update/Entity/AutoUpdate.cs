
namespace CBC.WebCore.WebSite.Update.Entity
{
    /// <summary>
    /// 静态页文件自动更新实体。
    /// </summary>
    /// <param name="watchers"></param>
    /// <param name="rules"></param>
    public class AutoUpdate(IList<Watcher> watchers, IList<Rule> rules)
    {
        /// <summary>
        /// 监视器集合。
        /// </summary>
        public IList<Watcher> Watchers { get { return watchers; } }

        /// <summary>
        /// 规则集合。
        /// </summary>
        public IList<Rule> Rules { get { return rules; } }
    }
}