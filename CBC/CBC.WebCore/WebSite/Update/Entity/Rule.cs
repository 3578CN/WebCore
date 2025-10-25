
namespace CBC.WebCore.WebSite.Update.Entity
{
    /// <summary>
    /// 页面匹配规则实体。
    /// </summary>
    /// <param name="path">匹配路径。</param>
    public class Rule(string path)
    {
        private DirectoryInfo m_Directory = new(path == "/" ? "/" : Path.GetDirectoryName(path));

        /// <summary>
        /// 匹配页面正则表达式。
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 匹配路径。
        /// </summary>
        public DirectoryInfo Directory
        {
            get
            {
                return m_Directory;
            }
            set
            {
                m_Directory = value;
            }
        }

        /// <summary>
        /// 是否包含子目录。
        /// </summary>
        public bool IncludeSubDirectories { get; set; }

        /// <summary>
        /// 更新时间间隔（秒数）。
        /// </summary>
        public int Interval { get; set; }
    }
}