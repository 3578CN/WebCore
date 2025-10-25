using System.Security.Claims;

namespace CBC.WebCore.Security
{
    /// <summary>
    /// 表示安全主体的抽象基类，继承自 <see cref="ClaimsPrincipal"/> 类，用于管理和验证用户的身份及角色信息。
    /// </summary>
    public class SecurityPrincipal : ClaimsPrincipal
    {
        /// <summary>
        /// 使用指定的 <see cref="ClaimsIdentity"/> 初始化 <see cref="SecurityPrincipal"/> 类的新实例。
        /// </summary>
        /// <param name="identity">包含用户声明的身份对象。</param>
        public SecurityPrincipal(ClaimsIdentity identity)
            : base(identity)
        {
        }

        /// <summary>
        /// 检查用户是否属于指定的角色。
        /// </summary>
        /// <param name="role">要检查的角色名称。</param>
        /// <returns>如果用户属于该角色则返回 true，否则返回 false。</returns>
        public bool IsInCBCRole(string role)
        {
            // 检查用户是否属于指定角色。
            return this.IsInRole(role);
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