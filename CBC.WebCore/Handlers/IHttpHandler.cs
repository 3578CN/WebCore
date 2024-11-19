using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.Handlers
{
    /// <summary>
    /// IHttpHandler 接口用于定义 HTTP 请求处理程序的基本结构。
    /// 通过实现此接口，类可以异步处理 HTTP 请求，并生成相应的响应。
    /// </summary>
    public interface IHttpHandler
    {
        /// <summary>
        /// 异步处理传入的 HTTP 请求。
        /// 实现此方法以执行请求处理的逻辑，并生成要返回给客户端的响应。
        /// </summary>
        /// <param name="context">
        /// HttpContext 对象，包含有关当前 HTTP 请求的所有信息，包括请求数据、响应对象、服务器变量等。
        /// </param>
        /// <returns>表示异步操作的 <see cref="Task"/>。</returns>
        Task ProcessRequestAsync(HttpContext context);

        /// <summary>
        /// 获取一个值，该值指示此处理程序是否可被重用。
        /// 如果此属性返回 true，则表示同一实例可以用于处理多个请求；否则为 false，表示每次请求都需要创建新的处理程序实例。
        /// </summary>
        bool IsReusable { get; }
    }
}