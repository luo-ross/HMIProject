using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RS.Commons.Helper
{
    public static class EncryptHelper
    {
        #region prop

        /// <summary>
        /// 默认编码
        /// </summary>
        public static Encoding DefaultEncoding { get; } = Encoding.UTF8;

        /// <summary>
        /// AES默认密钥  
        /// </summary>
        private const string AesDefaultKey = "A0541CDF2BCEFF8B3F345FB5690BE51C";



        /// <summary>
        /// DES默认密钥  
        /// </summary>
        private const string DesDefaultKey = "A1611A38";
        /// <summary>
        /// MD5加密  
        /// </summary>
        /// <param name="input">待加密字符串</param>
        /// <returns></returns>
        public static string MD5Encrypt(string input) //加密，不可逆  
        {
            try
            {
                using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
                {
                    return BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(input))).Replace("-", "");
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// AES加密 默认密钥  
        /// </summary>
        /// <param name="encryptString">待加密字符串</param>
        /// <returns></returns>
        public static string AESEncrypt(string encryptString)
        {
            return AESEncrypt(encryptString, AesDefaultKey);
        }


        /// <summary>
        /// AES解密 默认密钥  
        /// </summary>
        /// <param name="input">待解密字符串</param>
        /// <returns></returns>
        public static string AESDecrypt(string input)
        {
            return AESDecrypt(input, AesDefaultKey);
        }


        /// <summary>
        /// AES加密 自定义密钥  
        /// </summary>
        /// <param name="input">待加密字符串</param>
        /// <param name="theCipher">密钥</param>
        /// <returns></returns>
        public static string AESEncrypt(string input, string theCipher)
        {
            try
            {
                // 分组加密算法  
                SymmetricAlgorithm sa = Rijndael.Create();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(input);// 得到需要加密的字节数组      

                // 设置密钥及密钥向量  
                sa.Key = Encoding.UTF8.GetBytes(theCipher);
                sa.IV = Encoding.UTF8.GetBytes(MD5Encrypt(theCipher).Substring(0, 16));

                MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, sa.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    byte[] cipherBytes = ms.ToArray();// 得到加密后的字节数组  
                    return Convert.ToBase64String(cipherBytes);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// AES解密 自定义密钥  
        /// </summary>
        /// <param name="input">待解密字符串</param>
        /// <param name="theCipher">密钥</param>
        /// <returns></returns>
        public static string AESDecrypt(string input, string theCipher)
        {
            try
            {
                SymmetricAlgorithm sa = Rijndael.Create();
                sa.Key = Encoding.UTF8.GetBytes(theCipher);
                sa.IV = Encoding.UTF8.GetBytes(MD5Encrypt(theCipher).Substring(0, 16));
                byte[] decryptBytes = new byte[input.Length];

                MemoryStream ms = new MemoryStream(Convert.FromBase64String(input));
                using (CryptoStream cs = new CryptoStream(ms, sa.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                }

                return Encoding.UTF8.GetString(decryptBytes).TrimEnd('\0');
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion prop

        #region 通用加密算法

        /// <summary>
        /// 哈希加密算法
        /// </summary>
        /// <param name="hashAlgorithm"> 所有加密哈希算法实现均必须从中派生的基类 </param>
        /// <param name="input"> 待加密的字符串 </param>
        /// <param name="encoding"> 字符编码，为 null 时采用默认编码（UTF-8） </param>
        /// <returns></returns>
        private static string HashEncrypt(HashAlgorithm hashAlgorithm, string input, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            var data = hashAlgorithm.ComputeHash(encoding.GetBytes(input));

            return BitConverter.ToString(data).Replace("-", "");
        }

        /// <summary>
        /// 验证哈希值
        /// </summary>
        /// <param name="hashAlgorithm"> 所有加密哈希算法实现均必须从中派生的基类 </param>
        /// <param name="unhashedText"> 未加密的字符串 </param>
        /// <param name="hashedText"> 经过加密的哈希值 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        private static bool VerifyHashValue(HashAlgorithm hashAlgorithm, string unhashedText, string hashedText,
            Encoding encoding = null)
        {
            return string.Equals(HashEncrypt(hashAlgorithm, unhashedText, encoding), hashedText,
                StringComparison.OrdinalIgnoreCase);
        }

        #endregion 通用加密算法

        #region 哈希加密算法

        #region SHA1 算法

        /// <summary>
        /// SHA1 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="encoding"> 字符编码，为 null 时取默认值 </param>
        /// <returns></returns>
        public static string Sha1Encrypt(string input, Encoding encoding = null)
        {
            return HashEncrypt(SHA1.Create(), input, encoding);
        }

        /// <summary>
        /// 验证 SHA1 值
        /// </summary>
        /// <param name="input"> 未加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static bool VerifySha1Value(string input, Encoding encoding = null)
        {
            return VerifyHashValue(SHA1.Create(), input, Sha1Encrypt(input, encoding), encoding);
        }

        #endregion SHA1 算法

        #region SHA256 算法

        /// <summary>
        /// SHA256 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string Sha256Encrypt(string input, Encoding encoding = null)
        {
            return HashEncrypt(SHA256.Create(), input, encoding);
        }

        /// <summary>
        /// 验证 SHA256 值
        /// </summary>
        /// <param name="input"> 未加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static bool VerifySha256Value(string input, Encoding encoding = null)
        {
            return VerifyHashValue(SHA256.Create(), input, Sha256Encrypt(input, encoding), encoding);
        }

        #endregion SHA256 算法

        #region SHA384 算法

        /// <summary>
        /// SHA384 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string Sha384Encrypt(string input, Encoding encoding = null)
        {
            return HashEncrypt(SHA384.Create(), input, encoding);
        }

        /// <summary>
        /// 验证 SHA384 值
        /// </summary>
        /// <param name="input"> 未加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static bool VerifySha384Value(string input, Encoding encoding = null)
        {
            return VerifyHashValue(SHA256.Create(), input, Sha384Encrypt(input, encoding), encoding);
        }

        #endregion SHA384 算法

        #region SHA512 算法

        /// <summary>
        /// SHA512 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string Sha512Encrypt(string input, Encoding encoding = null)
        {
            return HashEncrypt(SHA512.Create(), input, encoding);
        }

        /// <summary>
        /// 验证 SHA512 值
        /// </summary>
        /// <param name="input"> 未加密的字符串 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static bool VerifySha512Value(string input, Encoding encoding = null)
        {
            return VerifyHashValue(SHA512.Create(), input, Sha512Encrypt(input, encoding), encoding);
        }

        #endregion SHA512 算法

        #region HMAC-MD5 加密

        /// <summary>
        /// HMAC-MD5 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="key"> 密钥 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string HmacMd5Encrypt(string input, string key, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            return HashEncrypt(new HMACMD5(encoding.GetBytes(key)), input, encoding);
        }

        #endregion HMAC-MD5 加密

        #region HMAC-SHA1 加密

        /// <summary>
        /// HMAC-SHA1 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="key"> 密钥 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string HmacSha1Encrypt(string input, string key, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            return HashEncrypt(new HMACSHA1(encoding.GetBytes(key)), input, encoding);
        }

        #endregion HMAC-SHA1 加密

        #region HMAC-SHA256 加密

        /// <summary>
        /// HMAC-SHA256 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="key"> 密钥 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string HmacSha256Encrypt(string input, string key, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = DefaultEncoding;

            return HashEncrypt(new HMACSHA256(encoding.GetBytes(key)), input, encoding);
        }

        #endregion HMAC-SHA256 加密

        #region HMAC-SHA384 加密

        /// <summary>
        /// HMAC-SHA384 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="key"> 密钥 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string HmacSha384Encrypt(string input, string key, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            return HashEncrypt(new HMACSHA384(encoding.GetBytes(key)), input, encoding);
        }

        #endregion HMAC-SHA384 加密

        #region HMAC-SHA512 加密

        /// <summary>
        /// HMAC-SHA512 加密
        /// </summary>
        /// <param name="input"> 要加密的字符串 </param>
        /// <param name="key"> 密钥 </param>
        /// <param name="encoding"> 字符编码 </param>
        /// <returns></returns>
        public static string HmacSha512Encrypt(string input, string key, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            return HashEncrypt(new HMACSHA512(encoding.GetBytes(key)), input, encoding);
        }

        #endregion HMAC-SHA512 加密

        #endregion 哈希加密算法

        #region 对称加密算法

        #region Des 加解密

        /// <summary>
        /// DES 加密
        /// </summary>
        /// <param name="input"> 待加密的字符串 </param>
        /// <param name="key"> 密钥（8位） </param>
        /// <param name="encoding">编码，为 null 取默认值</param>
        /// <returns></returns>
        public static string DesEncrypt(string input, string key = null, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            key = key ?? DesDefaultKey;
            try
            {
                var keyBytes = encoding.GetBytes(key);
                //var ivBytes = Encoding.UTF8.GetBytes(iv);
                var des = DES.Create();
                des.Mode = CipherMode.ECB; //兼容其他语言的 Des 加密算法
                using (var ms = new MemoryStream())
                {
                    var data = encoding.GetBytes(input);
                    byte[] ivBytes = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

                    using (var cs = new CryptoStream(ms, des.CreateEncryptor(keyBytes, ivBytes), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch
            {
                return input;
            }
        }

        /// <summary>
        /// DES 解密
        /// </summary>
        /// <param name="input"> 待解密的字符串 </param>
        /// <param name="key"> 密钥（8位） </param>
        /// <param name="encoding">编码，为 null 时取默认值</param>
        /// <returns></returns>
        public static string DesDecrypt(string input, string key = null, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            key = key ?? DesDefaultKey;

            var keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };

            var des = DES.Create();
            des.Mode = CipherMode.ECB; //兼容其他语言的Des加密算法
                                       // des.Padding = PaddingMode.Zeros; //自动补0

            using (var ms = new MemoryStream())
            {
                var data = Convert.FromBase64String(input);
                using (var cs = new CryptoStream(ms, des.CreateDecryptor(keyBytes, ivBytes), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                }
                return encoding.GetString(ms.ToArray());
            }

        }

        #endregion Des 加解密

        #endregion 对称加密算法

        #region 非对称加密算法

        /// <summary>
        /// 生成 RSA 公钥和私钥
        /// </summary>
        public static RsaKey GenerateRsaKeys()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                return new RsaKey()
                {
                    PublicKey = rsa.ToXmlString(false),
                    PrivateKey = rsa.ToXmlString(true)
                };
            }
        }


        /// <summary>
        /// RSA 加密
        /// </summary>
        /// <param name="publickey"> 公钥 </param>
        /// <param name="content"> 待加密的内容 </param>
        /// <param name="encoding">编码，为 null 时取默认编码</param>
        /// <returns> 经过加密的字符串 </returns>
        public static string RsaEncrypt(string publickey, string content, Encoding encoding = null)
        {
            encoding = encoding ?? DefaultEncoding;
            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                var inputBytes = encoding.GetBytes(content);
                rsaProvider.FromXmlString(publickey);
                int bufferSize = (rsaProvider.KeySize / 8) - 11;
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes), outputStream = new MemoryStream())
                {
                    while (true)
                    {
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }

                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var encryptedBytes = rsaProvider.Encrypt(temp, false);
                        outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
        }

        /// <summary>
        /// RSA 解密
        /// </summary>
        /// <param name="privatekey"> 私钥 </param>
        /// <param name="content"> 待解密的内容 </param>
        /// <param name="encoding"></param>
        /// <returns> 解密后的字符串 </returns>
        public static string RsaDecrypt(string privatekey, string content, Encoding encoding = null)
        {

            encoding = encoding ?? DefaultEncoding;
            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                var inputBytes = Convert.FromBase64String(content);
                rsaProvider.FromXmlString(privatekey);
                int bufferSize = rsaProvider.KeySize / 8;
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes),
                     outputStream = new MemoryStream())
                {
                    while (true)
                    {
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }
                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var rawBytes = rsaProvider.Decrypt(temp, false);
                        outputStream.Write(rawBytes, 0, rawBytes.Length);
                    }
                    return encoding.GetString(outputStream.ToArray());
                }
            }




        }

        #endregion 非对称加密算法



    }

    public struct RsaKey
    {
        /// <summary>
        /// 公钥
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// 私钥
        /// </summary>
        public string PrivateKey { get; set; }
    }

}
