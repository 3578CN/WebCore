namespace CBC.WebCore.WebSite
{
    #region 页面请求模式

    /// <summary>
    /// 页面请求模式。
    /// </summary>
    public enum RequestMode
    {
        /// <summary>
        /// 发布（.html）。
        /// </summary>
        Publish,

        /// <summary>
        /// 更新（.htmlu）。
        /// </summary>
        Update,

        /// <summary>
        /// 缓存（.htmlc）。
        /// </summary>
        Cache,

        /// <summary>
        /// 编辑（.htmle）。
        /// </summary>
        Edit,

        /// <summary>
        /// 保存（.htmls）。
        /// </summary>
        Save,

        /// <summary>
        /// 清空（.htmln）。
        /// </summary>
        Null,

        /// <summary>
        /// 删除（.htmld）。
        /// </summary>
        Delete
    }

    #endregion

    #region 页面请求类型

    /// <summary>
    /// 页面请求类型。
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// 静态页面。
        /// </summary>
        StaticPage,

        /// <summary>
        /// 动态页面。
        /// </summary>
        DynamicPage
    }

    #endregion

    #region 页面保存方法

    /// <summary>
    /// 页面保存方法。
    /// </summary>
    public enum SaveMethod
    {
        /// <summary>
        /// 保存为文件。
        /// </summary>
        Save,
        /// <summary>
        /// 保存到缓存。
        /// </summary>
        Cache,
        /// <summary>
        /// 不保存。
        /// </summary>
        None
    }

    #endregion
}