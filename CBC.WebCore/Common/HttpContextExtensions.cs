using System.Net;
using CBC.WebCore.Common.HttpHelpers;
using CBC.WebCore.Security.Entity;
using CBC.WebCore.WebSite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace CBC.WebCore.Common
{
    /// <summary>
    /// 提供对 HttpContext 扩展方法的集合，这些方法可用于处理常见的 HTTP 请求和响应任务。
    /// </summary>
    public static class HttpContextExtensions
    {
        #region 请求类型和请求模式相关方法

        internal const string Request_Url_Key = "Request_Url";

        /// <summary>
        /// 获取当前请求的请求类型。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文。</param>
        /// <returns>返回请求的类型。</returns>
        public static RequestType GetRequestType(this HttpContext context)
        {
            var urlDetails = context.GetUrlDetails();
            return urlDetails.RequestType;
        }

        /// <summary>
        /// 获取当前请求的请求模式。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文。</param>
        /// <returns>返回请求的模式。</returns>
        public static RequestMode GetRequestMode(this HttpContext context)
        {
            var urlDetails = context.GetUrlDetails();
            return urlDetails.RequestMode;
        }

        /// <summary>
        /// 获取并初始化包含请求 URL 详细信息的对象。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文。</param>
        /// <returns>返回初始化后的 URL 详细信息对象。</returns>
        private static UrlDetails GetUrlDetails(this HttpContext context)
        {
            if (context.Items[Request_Url_Key] == null)
            {
                var urlDetails = new UrlDetails();
                urlDetails.Initialize(context);

                context.Items.Add(Request_Url_Key, urlDetails);
            }
            return context.Items[Request_Url_Key] as UrlDetails;
        }

        #region 内部类

        /// <summary>
        /// 内部类，用于封装与 URL 相关的请求类型和请求模式。
        /// </summary>
        private class UrlDetails
        {
            private HttpContext _context;
            public RequestType RequestType { get; private set; }
            public RequestMode RequestMode { get; private set; }

            private static readonly Dictionary<string, (RequestType, RequestMode)> ExtensionMapping = new()
            {
                { ".html", (RequestType.StaticPage, RequestMode.Publish) },
                { ".htmlu", (RequestType.StaticPage, RequestMode.Update) },
                { ".htmlc", (RequestType.StaticPage, RequestMode.Cache) },
                { ".htmle", (RequestType.StaticPage, RequestMode.Edit) },
                { ".htmls", (RequestType.StaticPage, RequestMode.Save) },
                { ".htmln", (RequestType.StaticPage, RequestMode.Null) },
                { ".htmld", (RequestType.StaticPage, RequestMode.Delete) }
            };

            /// <summary>
            /// 初始化 UrlDetails 实例，根据 HTTP 请求上下文设置属性。
            /// </summary>
            /// <param name="context">当前的 HTTP 请求上下文。</param>
            public void Initialize(HttpContext context)
            {
                _context = context;
                // 根据请求的扩展名判断页面类型。
                if (ExtensionMapping.TryGetValue(context.GetRequestFileExtension(), out var requestSettings))
                {
                    // 静态页面。
                    RequestType = requestSettings.Item1;
                    RequestMode = requestSettings.Item2;
                }
                else
                {
                    // 动态页面。
                    RequestType = RequestType.DynamicPage;
                }
            }

            public override string ToString()
            {
                var uri = new Uri(_context.Request.GetDisplayUrl());
                // 只取 Scheme、Host 和 AbsolutePath，不包含 Query 和 Fragment。
                string urlWithoutQuery = $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
                return urlWithoutQuery;
            }
        }

        #endregion

        #endregion

        #region 获取客户端证书信息方法

        /// <summary>
        /// 获取客户端证书信息，包含证书的主题、颁发者、序列号、指纹、有效期等。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文。</param>
        /// <returns>如果证书有效，返回客户端证书实体对象，否则返回 null。</returns>
        public static ClientCert GetClientCert(this HttpContext context)
        {
            var clientCertificate = context.Connection.ClientCertificate;
            if (clientCertificate != null)
            {
                bool isValid = clientCertificate.NotBefore <= DateTime.Now && clientCertificate.NotAfter >= DateTime.Now;
                if (isValid)
                {
                    string subject = clientCertificate.Subject;
                    string issuer = clientCertificate.Issuer;
                    string serialNumber = clientCertificate.SerialNumber;
                    string thumbprint = clientCertificate.Thumbprint;
                    DateTime validFrom = clientCertificate.NotBefore;
                    DateTime validTo = clientCertificate.NotAfter;

                    // 进一步的身份验证或授权逻辑，例如检查证书是否是受信任的颁发者签发的等。

                    return new ClientCert(subject, issuer, serialNumber, thumbprint, validFrom, validTo);
                }
                else
                {
                    // 证书无效的处理逻辑。
                    return null;
                }
            }
            else
            {
                // 如果客户端没有提供证书的处理逻辑。
                return null;
            }
        }

        #endregion

        #region 获取请求地址相关方法

        /// <summary>
        /// 获取当前请求的完整 URL。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetFullUrl(this HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        }

        /// <summary>
        /// 根据请求路径，处理并返回绝对路径。
        /// - 如果路径以 .htmlu、.htmlc、.htmle、.htmls、.htmln、.htmld 结尾，替换为 .html。
        /// - 可选择返回文件系统中的绝对路径。
        /// </summary>
        /// <param name="context">当前的 HttpContext，用于获取请求的路径。</param>
        /// <param name="isFileSystemPath">是否返回文件系统中的绝对路径，默认为 false。</param>
        /// <returns>处理后的路径，如果匹配到特定扩展名则返回替换为 .html 的路径。</returns>
        public static string GetAbsolutePath(this HttpContext context, bool isFileSystemPath = false)
        {
            // 获取请求的路径。
            var path = context.Request.Path.Value;

            // 需要替换为 .html 的扩展名列表。
            ReadOnlySpan<string> extensionsToReplace = new[] { ".htmlu", ".htmlc", ".htmle", ".htmls", ".htmln", ".htmld" };

            // 如果路径不为空，进行处理。
            if (!string.IsNullOrEmpty(path))
            {
                // 遍历扩展名列表，匹配路径后缀。
                foreach (var ext in extensionsToReplace)
                {
                    if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        // 找到匹配的扩展名时，将其替换为 .html，并跳出循环。
                        path = string.Concat(path.AsSpan(0, path.Length - ext.Length), ".html");
                        break;
                    }
                }
            }

            // 如果需要返回文件系统中的绝对路径。
            if (isFileSystemPath)
            {
                // 获取根目录路径，并去掉末尾的斜杠。
                string webRootPath = HttpHelper.GetWebRootPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // 确保相对路径去掉前导斜杠。
                string relativePath = path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                // 返回文件系统的绝对路径。
                path = Path.Combine(webRootPath, relativePath);
            }

            // 返回最终的处理路径。
            return path;
        }

        /// <summary>
        /// 从当前请求的路径中提取并返回文件的扩展名。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文，包含请求的所有信息。</param>
        /// <returns>返回请求路径中的文件扩展名，如果路径中不包含扩展名，则返回空字符串。</returns>
        public static string GetRequestFileExtension(this HttpContext context)
        {
            // 获取请求路径。
            var path = context.Request.Path;

            // 使用 Path.GetExtension 提取扩展名。
            return Path.GetExtension(path);
        }

        /// <summary>
        /// 获取 Request Path 的最后一个部分，并加上 .html 后缀。
        /// </summary>
        /// <param name="context">Http 请求的上下文。</param>
        /// <returns>处理后的路径字符串，带有 .html 后缀。</returns>
        public static string GetProcessedPath(this HttpContext context)
        {
            // 获取请求路径。
            string path = context.Request.Path;

            // 使用 Path.GetFileName 方法获取最后一个路径部分。
            string lastSegment = System.IO.Path.GetFileName(path);

            // 拼接 .html 后缀。
            string result = $"{lastSegment}.html";

            return result;
        }


        /// <summary>
        /// 获取客户端的 IPv4 地址。如果客户端使用 IPv6 地址，将其转换为 IPv4。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文。</param>
        /// <returns>返回客户端的 IPv4 地址字符串，如果无法获取，则返回空字符串。</returns>
        public static string GetClientIpAddress(this HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;

            if (ipAddress != null)
            {
                // 如果是本地调试时的 IPv6 地址 (::1)，则转换为 IPv4 (127.0.0.1)
                if (ipAddress.IsIPv4MappedToIPv6 || ipAddress.Equals(IPAddress.IPv6Loopback))
                {
                    return "127.0.0.1";
                }

                // 如果是 IPv4 地址，直接返回
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipAddress.ToString();
                }
            }
            return "";
        }

        #endregion

        #region 重定向方法

        /// <summary>
        /// 执行 HTTP 307 Temporary Redirect（临时重定向），将客户端重定向到指定的 URL。
        /// </summary>
        /// <param name="context">当前的 HTTP 请求上下文。</param>
        /// <param name="url">要重定向到的目标 URL。</param>
        public static void Location(this HttpContext context, string url)
        {
            context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Response.Headers.Location = url;
        }

        #endregion

        #region 错误页处理方法

        /// <summary>
        /// 通用的错误响应方法，用于设置状态码并输出错误页面内容。
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文。</param>
        /// <param name="statusCode">要设置的 HTTP 状态码。</param>
        /// <param name="errorPageContent">要输出的自定义错误页面内容。如果为 null，则使用默认的错误页面内容。</param>
        /// <returns>异步任务。</returns>
        private static Task SetErrorAsync(HttpContext context, int statusCode, string errorPageContent)
        {
            // 设置 HTTP 响应的状态码。
            context.Response.StatusCode = statusCode;

            // 如果没有提供自定义的错误页面内容，则使用默认的内容。
            if (string.IsNullOrEmpty(errorPageContent))
            {
                errorPageContent = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "<h1>400 - 错误请求</h1><p>您的请求无效。</p>",
                    StatusCodes.Status401Unauthorized => "<h1>401 - 未授权</h1><p>需要进行身份验证。</p>",
                    StatusCodes.Status403Forbidden => "<h1>403 - 禁止访问</h1><p>您没有权限访问此资源。</p>",
                    StatusCodes.Status404NotFound => "<h1>404 - 未找到</h1><p>请求的资源无法找到。</p>",
                    StatusCodes.Status500InternalServerError => "<h1>500 - 服务器内部错误</h1><p>服务器发生了意外错误。</p>",
                    _ => "<h1>错误</h1><p>发生了一个错误。</p>"
                };
            }
            else
            {
                context.Response.ContentType = "text/plain; charset=utf-8";
            }

            // 将错误页面内容写入响应流
            return context.Response.WriteAsync(errorPageContent);
        }

        /// <summary>
        /// 设置 400 Bad Request（错误请求）的响应，并可选地输出自定义错误页面内容。
        /// 错误请求通常表示客户端发送的请求格式不正确或包含无效数据。
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文。</param>
        /// <param name="errorPageContent">可选的自定义错误页面内容。</param>
        /// <returns>异步任务。</returns>
        public static Task Error400Async(this HttpContext context, string errorPageContent = null)
        {
            return SetErrorAsync(context, StatusCodes.Status400BadRequest, errorPageContent);
        }

        /// <summary>
        /// 设置 401 Unauthorized（未授权）的响应，并可选地输出自定义错误页面内容。
        /// 未授权通常表示客户端请求资源需要身份验证，但未提供有效的认证信息。
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文。</param>
        /// <param name="errorPageContent">可选的自定义错误页面内容。</param>
        /// <returns>异步任务。</returns>
        public static Task Error401Async(this HttpContext context, string errorPageContent = null)
        {
            return SetErrorAsync(context, StatusCodes.Status401Unauthorized, errorPageContent);
        }

        /// <summary>
        /// 设置 403 Forbidden（禁止访问）的响应，并可选地输出自定义错误页面内容。
        /// 禁止访问表示服务器理解请求但拒绝执行，通常是因为权限不足。
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文。</param>
        /// <param name="errorPageContent">可选的自定义错误页面内容。</param>
        /// <returns>异步任务。</returns>
        public static Task Error403Async(this HttpContext context, string errorPageContent = null)
        {
            return SetErrorAsync(context, StatusCodes.Status403Forbidden, errorPageContent);
        }

        /// <summary>
        /// 设置 404 Not Found（未找到）的响应，并可选地输出自定义错误页面内容。
        /// 未找到表示服务器找不到请求的资源，通常是因为资源不存在或 URL 错误。
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文。</param>
        /// <param name="errorPageContent">可选的自定义错误页面内容。</param>
        /// <returns>异步任务。</returns>
        public static Task Error404Async(this HttpContext context, string errorPageContent = null)
        {
            return SetErrorAsync(context, StatusCodes.Status404NotFound, errorPageContent);
        }

        /// <summary>
        /// 设置 500 Internal Server Error（服务器内部错误）的响应，并可选地输出自定义错误页面内容。
        /// 服务器内部错误表示服务器在处理请求时遇到了意外情况，导致无法完成请求。
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文。</param>
        /// <param name="errorPageContent">可选的自定义错误页面内容。</param>
        /// <returns>异步任务。</returns>
        public static Task Error500Async(this HttpContext context, string errorPageContent = null)
        {
            return SetErrorAsync(context, StatusCodes.Status500InternalServerError, errorPageContent);
        }

        #endregion
    }
}