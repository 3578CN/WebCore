using System.Text;
using CBC.WebCore.Common;
using CBC.WebCore.Common.FileHelpers;
using Microsoft.AspNetCore.Http;

namespace CBC.WebCore.WebSite
{
    /// <summary>
    /// 异步页面过滤器类，用于拦截 Http 响应并进行处理。
    /// </summary>
    /// <remarks>
    /// 构造函数，初始化 PageFilter 并设置原始流。
    /// </remarks>
    /// <param name="context">HttpContext 对象。</param>
    /// <param name="originalBodyStream">原始响应流。</param>
    public class PageFilter(HttpContext context, Stream originalBodyStream) : Stream
    {
        private readonly MemoryStream _memoryStream = new();

        #region 属性

        /// <summary>
        /// 重写 CanRead 属性，表示流是否可读。
        /// </summary>
        public override bool CanRead => _memoryStream.CanRead;

        /// <summary>
        /// 重写 CanSeek 属性，表示流是否支持查找操作。
        /// </summary>
        public override bool CanSeek => _memoryStream.CanSeek;

        /// <summary>
        /// 重写 CanWrite 属性，表示流是否可写。
        /// </summary>
        public override bool CanWrite => _memoryStream.CanWrite;

        /// <summary>
        /// 重写 Length 属性，获取流的长度。
        /// </summary>
        public override long Length => _memoryStream.Length;

        /// <summary>
        /// 重写 Position 属性，获取或设置流的位置。
        /// </summary>
        public override long Position
        {
            get => _memoryStream.Position;
            set => _memoryStream.Position = value;
        }

        #endregion

        #region 重写其它 Stream 方法

        /// <summary>
        /// 重写 Flush 方法，刷新流。
        /// </summary>
        public override void Flush()
        {
            _memoryStream.Flush();
        }

        /// <summary>
        /// 重写 Read 方法，从流中读取字节。
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _memoryStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// 重写 Seek 方法，在流中查找位置。
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _memoryStream.Seek(offset, origin);
        }

        /// <summary>
        /// 重写 SetLength 方法，设置流的长度。
        /// </summary>
        public override void SetLength(long value)
        {
            _memoryStream.SetLength(value);
        }

        /// <summary>
        /// 重写 Write 方法，写入数据到流。
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _memoryStream.Write(buffer, offset, count);
        }

        #endregion

        #region 重写异步写入方法

        /// <summary>
        /// 异步写入方法，使用 ReadOnlyMemory/<byte/> 类型处理响应内容。
        /// 该方法将接收到的内容写入内存流，修改响应内容，并根据页面配置保存修改后的内容。
        /// </summary>
        /// <param name="buffer">响应内容的字节缓冲区。</param>
        /// <param name="cancellationToken">取消操作的标记。</param>
        /// <returns>表示异步操作的 ValueTask。</returns>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            // 将接收到的内容异步写入内存流，保存原始内容。
            await _memoryStream.WriteAsync(buffer, cancellationToken);

            // 将内存流的指针移回到流的开头，准备读取原始内容。
            _memoryStream.Seek(0, SeekOrigin.Begin);

            // 将内存流中的内容转换为字符串形式，用于进行内容修改。
            var responseBody = await new StreamReader(_memoryStream).ReadToEndAsync(cancellationToken);

            // 根据页面的保存方法进行相应的处理。
            switch (PageConfig.GetCurrent(context).SaveMethod)
            {
                case SaveMethod.Save:
                    // 在 </head> 前插入生成器信息，以标记该页面为保存状态。
                    responseBody = responseBody.Replace("</head>", $"<meta name=\"Generator\" content=\"CBC.WebCore 1.0,Save,{DateTime.Now:yyyy-MM-dd HH:mm:ss:fffff}\" />\r\n</head>");

                    // 如果请求模式是保存或更新，将修改后的内容保存到文件。
                    if (context.GetRequestMode() == RequestMode.Publish || context.GetRequestMode() == RequestMode.Save || context.GetRequestMode() == RequestMode.Update)
                    {
                        try
                        {
                            // 获取保存路径。
                            string savePath = context.GetAbsolutePath(true);

                            // 保存文件。
                            await FileHelper.WriteFileAsync(savePath, responseBody);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"生成静态页 HTML 文件时出现错误: {ex.Message}");
                        }
                    }
                    break;

                case SaveMethod.Cache:
                    // 在 </head> 前插入缓存信息，用于标识该页面为缓存状态。
                    responseBody = responseBody.Replace("</head>", $"<meta name=\"Generator\" content=\"CBC.WebCore 1.0,Cache,{DateTime.Now:yyyy-MM-dd HH:mm:ss:fffff}\" />\r\n</head>");
                    break;

                case SaveMethod.None:
                    // 如果没有指定保存方法，则插入无操作标识。
                    responseBody = responseBody.Replace("</head>", $"<meta name=\"Generator\" content=\"CBC.WebCore 1.0,None,{DateTime.Now:yyyy-MM-dd HH:mm:ss:fffff}\" />\r\n</head>");
                    break;
            }

            // 将修改后的响应内容转换为字节数组，准备发送给客户端。
            var modifiedBytes = Encoding.UTF8.GetBytes(responseBody);

            // 释放内存流。
            _memoryStream?.Dispose();

            // 将修改后的内容写入原始响应流，确保客户端收到更新后的数据。
            await originalBodyStream.WriteAsync(modifiedBytes, cancellationToken);
            await originalBodyStream.FlushAsync(cancellationToken);  // 刷新原始响应流，确保数据发送到客户端。
        }

        #endregion
    }
}