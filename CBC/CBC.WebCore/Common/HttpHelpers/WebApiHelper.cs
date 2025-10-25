using System.Text;
using System.Text.Json;

namespace CBC.WebCore.Common.HttpHelpers
{
    /// <summary>
    /// WebApiHelper 类用于封装常见的 Web API 操作，包括发送 GET 和 POST 请求。
    /// </summary>
    public class WebApiHelper
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 构造函数，初始化 HttpClient 实例。
        /// </summary>
        public WebApiHelper()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// 发送 GET 请求并返回指定类型的结果。
        /// </summary>
        /// <typeparam name="T">返回结果的类型。</typeparam>
        /// <param name="url">请求的 URL。</param>
        /// <returns>返回指定类型的结果。</returns>
        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// 发送 POST 请求，并将请求体序列化为 JSON 格式。
        /// </summary>
        /// <typeparam name="TRequest">请求体的类型。</typeparam>
        /// <typeparam name="TResponse">返回结果的类型。</typeparam>
        /// <param name="url">请求的 URL。</param>
        /// <param name="data">请求体的数据。</param>
        /// <returns>返回指定类型的结果。</returns>
        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseString);
        }

        /// <summary>
        /// 释放 HttpClient 实例占用的资源。
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
