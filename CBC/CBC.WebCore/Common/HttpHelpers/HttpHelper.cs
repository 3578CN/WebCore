using Microsoft.AspNetCore.Hosting;

namespace CBC.WebCore.Common.HttpHelpers
{
    /// <summary>
    /// Http 操作方法类。
    /// </summary>
    public class HttpHelper
    {
        private static IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// 初始化 HttpHelper，注入 IWebHostEnvironment。
        /// </summary>
        /// <param name="webHostEnvironment">Web 主机环境。</param>
        public static void Initialize(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 获取 wwwroot 文件夹的绝对路径。
        /// </summary>
        /// <returns>wwwroot 文件夹的绝对路径。</returns>
        public static string GetWebRootPath()
        {
            return _webHostEnvironment.WebRootPath;
        }

        /// <summary>
        /// 获取指定虚拟路径的绝对路径。
        /// </summary>
        /// <param name="virtualPath">虚拟路径，例如 "/template/"。</param>
        /// <returns>返回对应的绝对路径。</returns>
        public static string MapPath(string virtualPath)
        {
            return Path.GetFullPath(Path.Combine(_webHostEnvironment.ContentRootPath, virtualPath));
        }
    }

    /// <summary>
    /// 用于初始化 HttpHelper 类。
    /// </summary>
    public class HttpInitializer
    {
        /// <summary>
        /// 构造函数，注入 IWebHostEnvironment。
        /// </summary>
        /// <param name="webHostEnvironment">Web 主机环境。</param>
        public HttpInitializer(IWebHostEnvironment webHostEnvironment)
        {
            HttpHelper.Initialize(webHostEnvironment);
        }
    }
}