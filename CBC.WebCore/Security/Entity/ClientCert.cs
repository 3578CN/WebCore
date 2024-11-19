namespace CBC.WebCore.Security.Entity
{
    /// <summary>
    /// 客户端证书字段实体。
    /// </summary>
    /// <remarks>
    /// 构造函数，用于初始化证书信息。
    /// </remarks>
    /// <param name="subject">证书持有者的主题信息。</param>
    /// <param name="issuer">颁发证书的实体的名称或标识。</param>
    /// <param name="serialNumber">证书的唯一序列号。</param>
    /// <param name="thumbprint">证书的指纹或摘要。</param>
    /// <param name="notBefore">证书的有效起始日期。</param>
    /// <param name="notAfter">证书的有效结束日期。</param>
    public class ClientCert(string subject, string issuer, string serialNumber, string thumbprint, DateTime notBefore, DateTime notAfter)
    {
        /// <summary>
        /// 获取证书持有者的主题信息。
        /// </summary>
        public string Subject { get; } = subject;

        /// <summary>
        /// 获取颁发证书的实体的名称或标识。
        /// </summary>
        public string Issuer { get; } = issuer;

        /// <summary>
        /// 获取证书的唯一序列号。
        /// </summary>
        public string SerialNumber { get; } = serialNumber;

        /// <summary>
        /// 获取证书的指纹或摘要。
        /// </summary>
        public string Thumbprint { get; } = thumbprint;

        /// <summary>
        /// 获取证书的有效起始日期。
        /// </summary>
        public DateTime NotBefore { get; } = notBefore;

        /// <summary>
        /// 获取证书的有效结束日期。
        /// </summary>
        public DateTime NotAfter { get; } = notAfter;
    }
}