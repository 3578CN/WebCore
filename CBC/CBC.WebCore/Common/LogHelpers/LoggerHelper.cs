using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CBC.WebCore.Common.LogHelpers
{
    /// <summary>
    /// 日志帮助类，提供静态方法记录信息、警告和错误日志。
    /// </summary>
    public static class LoggerHelper
    {
        // 用于缓存 CBCLogger 实例的线程安全字典。
        private static readonly ConcurrentDictionary<Type, ILogger> _loggerCache = new ConcurrentDictionary<Type, ILogger>();

        // 用于保存 ILoggerFactory 实例的静态字段。
        private static ILoggerFactory _loggerFactory;

        /// <summary>
        /// 使用 ILoggerFactory 实例配置 CBCLogger。
        /// 此方法必须在应用程序启动期间调用以初始化日志记录工厂。
        /// </summary>
        /// <param name="loggerFactory">用于创建日志记录器的 ILoggerFactory 实例。</param>
        public static void Configure(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// 记录信息日志。
        /// </summary>
        /// <param name="message">要记录的信息消息。</param>
        public static void Information(string message)
        {
            var logger = GetLoggerForCaller();
            logger.LogInformation(message);
        }

        /// <summary>
        /// 记录警告日志。
        /// </summary>
        /// <param name="message">要记录的警告消息。</param>
        public static void Warning(string message)
        {
            var logger = GetLoggerForCaller();
            logger.LogWarning(message);
        }

        /// <summary>
        /// 记录错误日志。
        /// </summary>
        /// <param name="message">要记录的错误消息。</param>
        public static void Error(string message)
        {
            var logger = GetLoggerForCaller();
            logger.LogError(message);
        }

        /// <summary>
        /// 获取调用者的 CBCLogger 实例，自动缓存结果以提高性能。
        /// </summary>
        /// <returns>调用者类型对应的 ILogger 实例。</returns>
        private static ILogger GetLoggerForCaller()
        {
            // 获取调用者的类类型。
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(2); // 获取上两层调用者的信息。
            var method = frame.GetMethod();
            var callerType = method.DeclaringType;

            // 检查缓存中是否已经存在该类型的 CBCLogger 实例。
            return _loggerCache.GetOrAdd(callerType, type => _loggerFactory.CreateLogger(type));
        }
    }
}