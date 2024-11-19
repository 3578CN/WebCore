using System.Net;

namespace CBC.WebCore.Common.NetworkHelpers
{
    /// <summary>
    /// IP 地址操作方法类。
    /// </summary>
    public static class IpAddressHelper
    {
        #region 计算 IP 地址数量

        /// <summary>
        /// 计算 IP 地址数量。
        /// </summary>
        /// <param name="ipAddress">IP 地址网络号。</param>
        /// <returns>返回数量。</returns>
        public static long IPCount(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] streachip = ipAddress.Split('.');
                long intcip = 0;
                for (int i = 0; i < 4; i++)
                {
                    intcip += (long)(int.Parse(streachip[i]) * Math.Pow(256, 3 - i));
                }
                return intcip;
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region 检查一个 IP 地址是否在一个 IP 地址段中

        /// <summary>
        /// 检查一个 IP 地址是否在一个 IP 地址段中。
        /// </summary>
        /// <param name="address">要检查的 IP 地址。</param>
        /// <param name="ipSegment">IP 地址段。</param>
        /// <param name="maskAddress">子网掩码。</param>
        /// <returns>返回一个布尔值。</returns>
        public static bool IsInAnIPSegment(string address, string ipSegment, string maskAddress)
        {
            long ip0 = IPAddress.Parse(maskAddress).ToLong();
            long ip1 = IPAddress.Parse(address).ToLong();
            long ip2 = IPAddress.Parse(ipSegment).ToLong();

            return (ip0 & ip1) == (ip0 & ip2);
        }

        #endregion

        #region IP 地址与无符号整数之间的转换

        /// <summary>
        /// 无符号整数转换为 IP 地址字符串。
        /// </summary>
        /// <param name="ipId">IP 地址编号。</param>
        /// <returns>返回一个字符串。</returns>
        public static string IntToIP(uint ipId)
        {
            byte a = (byte)((ipId & 0xFF000000) >> 0x18);
            byte b = (byte)((ipId & 0x00FF0000) >> 0xF);
            byte c = (byte)((ipId & 0x0000FF00) >> 0x8);
            byte d = (byte)(ipId & 0x000000FF);
            string ipStr = string.Format("{0}.{1}.{2}.{3}", a, b, c, d);
            return ipStr;
        }

        /// <summary>
        /// IP 地址字符串转换为无符号整数。
        /// </summary>
        /// <param name="ipAddress">IP 地址字符串</param>
        /// <returns>返回一个无符号整。</returns>
        public static uint IPToInt(string ipAddress)
        {
            string[] ip = ipAddress.Split('.');
            uint ipId = 0xFFFFFF00 | byte.Parse(ip[3]);
            ipId = ipId & 0xFFFF00FF | uint.Parse(ip[2]) << 0x8;
            ipId = ipId & 0xFF00FFFF | uint.Parse(ip[1]) << 0xF;
            ipId = ipId & 0x00FFFFFF | uint.Parse(ip[0]) << 0x18;
            return ipId;
        }

        #endregion

        #region 获取计算机的 IP 地址列表

        /// <summary>
        /// 获取本地计算机的 IP 地址列表。
        /// </summary>
        /// <returns>返回一个数组。</returns>
        public static string[] GetIPAddress()
        {
            IPAddress[] AddressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

            int IPCount = AddressList.Length;
            string[] IP = new string[IPCount];

            for (int i = 0; i < IPCount; i++)
            {
                IP[i] = AddressList[i].ToString();
            }
            return IP;
        }

        /// <summary>
        /// 获取指定主机名或 IP 的计算机的 IP 地址列表。
        /// </summary>
        /// <param name="hostName">主机名或 IP。</param>
        /// <returns>返回一个数组。</returns>
        public static string[] GetIPAddress(string hostName)
        {
            IPAddress[] AddressList = Dns.GetHostEntry(hostName).AddressList;

            int IPCount = AddressList.Length;
            string[] IP = new string[IPCount];

            for (int i = 0; i < IPCount; i++)
            {
                IP[i] = AddressList[i].ToString();
            }
            return IP;
        }

        #endregion

        #region 扩展方法

        /// <summary>
        /// 返回 Long 类型的数据。
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long ToLong(this IPAddress ip)
        {
            int x = 3;
            long o = 0;
            foreach (byte f in ip.GetAddressBytes())
            {
                o += (long)f << 8 * x--;
            }
            return o;
        }

        /// <summary>
        /// 返回 IPAddress 类型的数据。
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static IPAddress ToIPAddress(this long l)
        {
            var b = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                b[3 - i] = (byte)(l >> 8 * i & 255);
            }
            return new IPAddress(b);
        }

        #endregion
    }
}