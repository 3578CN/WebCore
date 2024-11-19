namespace CBC.WebCore.Security.Entity
{
    /// <summary>
    /// 安全验证实体。
    /// </summary>
    /// <param name="type"></param>
    /// <param name="login"></param>
    /// <param name="authorizedPaths"></param>
    /// <param name="unrestrictedPaths"></param>
    /// <param name="staticPageManagementIPs"></param>
    public class Security(string type, string login, IList<string> authorizedPaths, IList<string> unrestrictedPaths, IDictionary<string, string> staticPageManagementIPs)
    {
        /// <summary>
        /// 获取用户身份信息操作方法处理程序的路径。
        /// </summary>
        public string Type => type;

        /// <summary>
        /// 登录页面。
        /// </summary>
        public string Login => login;

        /// <summary>
        /// 需要授权访问的路径。
        /// </summary>
        public IList<string> AuthorizedPaths => authorizedPaths;

        /// <summary>
        /// 无需授权即可访问的路径。
        /// </summary>
        public IList<string> UnrestrictedPaths => unrestrictedPaths;

        /// <summary>
        /// 静态页面管理的 IP 地址。
        /// </summary>
        public IDictionary<string, string> StaticPageManagementIPs => staticPageManagementIPs;
    }
}