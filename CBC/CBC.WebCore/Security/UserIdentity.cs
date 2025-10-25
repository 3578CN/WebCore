using System.Security.Claims;

namespace CBC.WebCore.Security
{
    /// <summary>
    /// 表示用户身份的自定义实现，继承自 <see cref="ClaimsIdentity"/> 类，用于管理用户的身份认证及声明信息。
    /// </summary>
    public class UserIdentity : ClaimsIdentity
    {
        /// <summary>
        /// 使用指定的认证类型和声明列表初始化 <see cref="UserIdentity"/> 类的新实例。
        /// </summary>
        /// <param name="claims">包含用户声明的列表。</param>
        /// <param name="authenticationType">认证类型，用于标识认证的方式。</param>
        public UserIdentity(List<Claim> claims, string authenticationType)
            : base(claims, authenticationType)
        {
        }

        /// <summary>
        /// 添加自定义声明到当前身份对象中。
        /// </summary>
        /// <param name="type">声明的类型，通常是表示声明性质的字符串。</param>
        /// <param name="value">声明的值，对应于声明类型所表达的内容。</param>
        public void AddClaim(string type, string value)
        {
            // 创建新的 Claim 并添加到当前对象中。
            this.AddClaim(new Claim(type, value));
        }

        /// <summary>
        /// 从当前身份对象中移除指定类型的声明。
        /// </summary>
        /// <param name="type">要移除的声明类型。</param>
        public void RemoveClaim(string type)
        {
            // 查找并移除指定类型的声明。
            var claim = this.FindFirst(type);
            if (claim != null)
            {
                this.RemoveClaim(claim);
            }
        }

        /// <summary>
        /// 获取指定类型的声明值。
        /// </summary>
        /// <param name="type">声明的类型，表示需要检索的声明。</param>
        /// <returns>返回声明的值，如果未找到则返回 null。</returns>
        public string GetClaimValue(string type)
        {
            // 查找并返回指定类型的声明值。
            return this.FindFirst(type)?.Value;
        }
    }
}