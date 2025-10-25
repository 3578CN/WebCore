using Microsoft.Extensions.Caching.Memory;

namespace CBC.WebCore.Common.CacheHelpers
{
    /// <summary>
    /// 内存缓存操作方法类。
    /// </summary>
    public class CacheHelper
    {
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 获取当前 CBCCache 实例。
        /// </summary>
        public static CacheHelper Current { get; private set; }

        /// <summary>
        /// 初始化 <see cref="CacheHelper"/> 类的新实例。
        /// </summary>
        /// <param name="cache">内存缓存实例。</param>
        public CacheHelper(IMemoryCache cache)
        {
            _cache = cache;
            Current = this; // 设置静态属性 Current 为当前实例。
        }

        /// <summary>
        /// 添加缓存项。
        /// </summary>
        /// <typeparam name="T">缓存项的类型。</typeparam>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">缓存项的值。</param>
        /// <param name="absoluteExpirationRelativeToNow">相对于现在的绝对过期时间。</param>
        public void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();

            if (absoluteExpirationRelativeToNow.HasValue)
            {
                cacheEntryOptions.SetAbsoluteExpiration(absoluteExpirationRelativeToNow.Value);
            }

            _cache.Set(key, value, cacheEntryOptions);
        }

        /// <summary>
        /// 获取缓存项。
        /// </summary>
        /// <typeparam name="T">缓存项的类型。</typeparam>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">输出参数，缓存项的值。</param>
        /// <returns>如果缓存项存在则返回 true，否则返回 false。</returns>
        public bool Get<T>(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        /// <summary>
        /// 移除缓存项。
        /// </summary>
        /// <param name="key">缓存项的键。</param>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}