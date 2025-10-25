using System.Text;

namespace CBC.WebCore.Common.FileHelpers
{
    /// <summary>
    /// 文件系统对象操作方法类。
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 异步写入指定内容到文件中。如果文件不存在，则会创建该文件；
        /// 如果目录不存在，也会自动创建目录。支持追加或覆盖写入方式。
        /// </summary>
        /// <param name="file">文件的完整路径，包括文件名。</param>
        /// <param name="content">需要写入的字符串内容。</param>
        /// <param name="isAppend">是否以追加的方式写入。如果为 true，内容会追加到文件末尾；如果为 false，文件内容会被覆盖。</param>
        /// <returns>表示异步操作的 <see cref="Task"/>。</returns>
        /// <exception cref="ArgumentException">当文件路径为空时抛出。</exception>
        /// <exception cref="UnauthorizedAccessException">当没有权限写入文件时抛出。</exception>
        /// <exception cref="DirectoryNotFoundException">当路径中的某个目录不存在且无法创建时抛出。</exception>
        /// <exception cref="IOException">当发生 I/O 错误时抛出。</exception>
        public static async Task WriteFileAsync(string file, string content, bool isAppend = false)
        {
            // 获取文件信息。
            var fileInfo = new FileInfo(file);

            // 如果文件所在目录不存在，创建该目录。
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            // 使用 `using` 语句确保文件流自动释放，避免手动调用 `Dispose()`。
            using var fileStream = new FileStream(file, isAppend ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            // 将字符串转换为字节数组。
            var fileBytes = Encoding.UTF8.GetBytes(content);

            // 异步写入字节数据到文件流。
            await fileStream.WriteAsync(fileBytes);

            // 异步刷新文件流，确保数据立即保存到磁盘。
            await fileStream.FlushAsync();
        }

        /// <summary>
        /// 异步读取文件的内容，并返回包含该内容的 <see cref="StringBuilder"/> 对象。
        /// 如果文件不存在或无法访问，会抛出相应的异常。
        /// </summary>
        /// <param name="file">文件的完整路径，包括文件名。</param>
        /// <returns>异步操作，返回包含读取到的文件内容的 <see cref="StringBuilder"/> 对象。</returns>
        /// <exception cref="FileNotFoundException">当指定文件不存在时抛出。</exception>
        /// <exception cref="UnauthorizedAccessException">当没有权限读取文件时抛出。</exception>
        /// <exception cref="IOException">当发生 I/O 错误时抛出。</exception>
        public static async Task<StringBuilder> ReadFileAsync(string file)
        {
            // 使用 `using` 确保 `StreamReader` 在操作完成后释放资源。
            using var streamReader = new StreamReader(file, Encoding.UTF8);
            // 异步读取整个文件的内容。
            string content = await streamReader.ReadToEndAsync();

            // 返回读取的内容，并将其封装在 `StringBuilder` 中。
            return new StringBuilder(content);
        }

        /// <summary>
        /// 删除指定路径下的文件，并返回是否删除成功。
        /// </summary>
        /// <param name="file">文件的完整路径，包括文件名。</param>
        /// <returns>布尔值，表示文件是否删除成功。</returns>
        /// <exception cref="UnauthorizedAccessException">当没有权限删除文件时抛出。</exception>
        /// <exception cref="FileNotFoundException">当文件不存在时抛出。</exception>
        /// <exception cref="IOException">当文件正在被使用或发生 I/O 错误时抛出。</exception>
        public static bool DeleteFile(string file)
        {
            // 检查文件是否存在，如果不存在直接返回 false。
            if (File.Exists(file))
            {
                File.Delete(file); // 删除文件。
            }
            else
            {
                return false; // 文件不存在，删除失败。
            }

            return true; // 文件删除成功。
        }
    }
}