
namespace CBC.WebCore.WebSite.Update.Entity
{
    /// <summary>
    /// 监视器实体。
    /// </summary>
    public class Watcher
    {
        /// <summary>
        /// 监视类型。
        /// </summary>
        public string EventTypes { get; set; }

        /// <summary>
        /// 监视路径。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 是否包含子目录。
        /// </summary>
        public bool IncludeSubDirectories { get; set; }
    }
}