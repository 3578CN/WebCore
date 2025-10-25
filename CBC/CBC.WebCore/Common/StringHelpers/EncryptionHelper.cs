using System.Security.Cryptography;
using System.Text;

namespace CBC.WebCore.Common.StringHelpers
{
    /// <summary>
    /// 字符串加密解密方法类。
    /// </summary>
    public class EncryptionHelper
    {
        #region 16进制转化

        /// <summary>
        /// String 转换 Base 16。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToBase16(string str)
        {
            StringBuilder ret = new StringBuilder();
            foreach (byte b in Encoding.UTF8.GetBytes(str))
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// Base 16 转换 String。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base16ToString(string str)
        {
            try
            {
                byte[] inputByteArray = new byte[str.Length / 2];
                for (int x = 0; x < str.Length / 2; x++)
                {
                    int i = Convert.ToInt32(str.Substring(x * 2, 2), 16);
                    inputByteArray[x] = (byte)i;
                }
                return Encoding.UTF8.GetString(inputByteArray);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region AES 加密解密方法

        /// <summary>
        /// AES 加密。
        /// </summary>
        /// <param name="plaintext">明文。</param>
        /// <param name="key">密钥。</param>
        /// <param name="iv">向量。</param>
        /// <returns>返回加密后的密文。</returns>
        public static string AESEncrypt(string plaintext, string key, string iv)
        {
            if (string.IsNullOrEmpty(plaintext))
            {
                return string.Empty;
            }

            byte[] bKey = Encoding.UTF8.GetBytes(StringHelper.UTF8_MD5Encrypt(key).Substring(0, 32));
            byte[] bIV = Encoding.UTF8.GetBytes(StringHelper.UTF8_MD5Encrypt(iv).Substring(0, 16));
            byte[] byteArray = Encoding.UTF8.GetBytes(plaintext);

            using (Aes aes = Aes.Create())
            {
                aes.Key = bKey;
                aes.IV = bIV;
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        return StringToBase16(Convert.ToBase64String(mStream.ToArray()));
                    }
                }
            }
        }

        /// <summary>
        /// AES 解密。
        /// </summary>
        /// <param name="ciphertext">密文。</param>
        /// <param name="key">密钥。</param>
        /// <param name="iv">向量。</param>
        /// <returns>返回解密后的明文字符串。</returns>
        public static string AESDecrypt(string ciphertext, string key, string iv)
        {
            //16进制转换。
            ciphertext = Base16ToString(ciphertext);
            if (string.IsNullOrEmpty(ciphertext))
            {
                return string.Empty;
            }

            byte[] bKey = Encoding.UTF8.GetBytes(StringHelper.UTF8_MD5Encrypt(key).Substring(0, 32));
            byte[] bIV = Encoding.UTF8.GetBytes(StringHelper.UTF8_MD5Encrypt(iv).Substring(0, 16));
            byte[] byteArray = Convert.FromBase64String(ciphertext);

            using (Aes aes = Aes.Create())
            {
                aes.Key = bKey;
                aes.IV = bIV;
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        return Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
        }

        #endregion
    }
}