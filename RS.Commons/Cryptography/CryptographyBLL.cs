using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Commons.Cryptography;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.Models;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Xml;

namespace RS.Commons
{
    /// <summary>
    /// 数据加解密服务实现类
    /// </summary>
    [ServiceInjectConfig(typeof(ICryptographyBLL), ServiceLifetime.Singleton)]
    public class CryptographyBLL : ICryptographyBLL
    {
        private readonly ILogBLL LogBLL;
        private readonly IDataProtector DataProtector;
        private readonly IMemoryCache MemoryCache;
        public CryptographyBLL(ILogBLL logBLL, IDataProtectionProvider dataProtectionProvider, IMemoryCache memoryCache)
        {
            LogBLL = logBLL;
            DataProtector = dataProtectionProvider.CreateProtector("15B4D612-39AB-45AF-8CEF-BC4FAF711D1C");
            MemoryCache = memoryCache;
        }

        #region AES加解密
        /// <summary>
        /// 获取会话数据
        /// </summary>
        /// <returns></returns>
        public OperateResult<SessionModel> GetSessionModel()
        {
            //先从内存里获取Aes秘钥
            MemoryCache.TryGetValue(MemoryCacheKey.SessionModelKey, out SessionModel sessionModel);

            if (sessionModel == null)
            {
                return OperateResult.CreateFailResult<SessionModel>("你暂时无法使用该软件！");
            }

            if (string.IsNullOrEmpty(sessionModel.AesKey) || string.IsNullOrWhiteSpace(sessionModel.AesKey) || sessionModel.AesKey.Length != 43)
            {
                return OperateResult.CreateFailResult<SessionModel>("密钥不合法");
            }

            //从内存里获取AppId
            if (string.IsNullOrEmpty(sessionModel.AppId) || string.IsNullOrWhiteSpace(sessionModel.AppId))
            {
                return OperateResult.CreateFailResult<SessionModel>("没有获取到正确AppID");
            }

            //从内存里获取Token

            if (string.IsNullOrEmpty(sessionModel.Token) || string.IsNullOrWhiteSpace(sessionModel.Token))
            {
                return OperateResult.CreateFailResult<SessionModel>("没有获取到正确Token");
            }
            return OperateResult.CreateSuccessResult(sessionModel);
        }

        /// <summary>
        /// AES对称解密
        /// </summary>
        /// <typeparam name="TResult">解密实体类型</typeparam>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        public OperateResult<TResult> AESDecrypt<TResult>(AESEncryptModel aesEncryptModel)
        {
            //获取 aesKey,  appId,  token
            var getSessionModelResult = GetSessionModel();
            if (!getSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(getSessionModelResult);
            }
            var sessionModel = getSessionModelResult.Data;

            //返回AES对称解密数据
            return AESDecrypt<TResult>(aesEncryptModel, sessionModel);
        }

        /// <summary>
        /// AES对称数据解密
        /// </summary>
        /// <typeparam name="TResult">返回实体类型</typeparam>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionModel">会话实体类</param>
        /// <returns></returns>
        public OperateResult<TResult> AESDecrypt<TResult>(AESEncryptModel aesEncryptModel, SessionModel sessionModel)
        {
            //验证签名
            var verifySignatureResult = this.VerifySignature(sessionModel.Token, aesEncryptModel.TimeStamp, aesEncryptModel.Nonce, aesEncryptModel.Encrypt, aesEncryptModel.MsgSignature);
            if (!verifySignatureResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<TResult>(verifySignatureResult);
            }

            //解密
            string appid = "";
            string sMsg;
            try
            {
                sMsg = this.AESDecrypt(aesEncryptModel.Encrypt, sessionModel.AesKey, ref appid);
            }
            catch (FormatException)
            {
                return OperateResult.CreateFailResult<TResult>("解码Base64错误");
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<TResult>("数据解密出错");
            }

            if (appid != sessionModel.AppId)
            {
                return OperateResult.CreateFailResult<TResult>("验证AppID失败");
            }

            return OperateResult.CreateSuccessResult(sMsg.ToObject<TResult>());
        }

        /// <summary>
        /// 生成AES对称密钥
        /// </summary>
        /// <returns></returns>
        public string GenerateAESKey()
        {
            Aes aes = Aes.Create();
            var keyString = Convert.ToBase64String(aes.Key);
            keyString = keyString.Substring(0, keyString.Length - 1);
            return keyString;
        }

        /// <summary>
        /// AES对称加密
        /// </summary>
        /// <typeparam name="T">待加密数据类型</typeparam>
        /// <param name="encryptModelShould">待加密数据</param>
        /// <returns></returns>
        public OperateResult<AESEncryptModel> AESEncrypt<T>(T encryptModelShould)
        {
            //获取 aesKey,  appId,  token
            var getSessionModelResult = GetSessionModel();
            if (!getSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getSessionModelResult);
            }
            var sessionModel = getSessionModelResult.Data;

            //返回AES对称加密数据
            return AESEncrypt(encryptModelShould, sessionModel);
        }

        /// <summary>
        /// AES对称加密
        /// </summary>
        /// <typeparam name="T">待加密数据类型</typeparam>
        /// <param name="encryptModelShould">待加密数据</param>
        /// <param name="sessionModel">会话实体</param>
        /// <returns></returns>
        public OperateResult<AESEncryptModel> AESEncrypt<T>(T encryptModelShould, SessionModel sessionModel)
        {

            if (encryptModelShould == null)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>("加密实体不能为空");
            }

            string replyMsg = encryptModelShould.ToJson();
            string timeStamp = DateTime.UtcNow.ToTimeStampString();
            string nonce = this.CreateRandCode(10);

            string raw = "";
            try
            {
                raw = this.AESEncrypt(replyMsg, sessionModel.AesKey, sessionModel.AppId);
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>("数据加密错误");
            }

            //生成签名
            var genarateSinatureResult = this.GenarateSinature(sessionModel.Token, timeStamp, nonce, raw);
            if (!genarateSinatureResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(genarateSinatureResult);
            }
            string MsgSigature = genarateSinatureResult.Data;

            AESEncryptModel aesEncryptModel = new AESEncryptModel();
            aesEncryptModel.Encrypt = raw;
            aesEncryptModel.MsgSignature = MsgSigature;
            aesEncryptModel.TimeStamp = timeStamp;
            aesEncryptModel.Nonce = nonce;
            return OperateResult.CreateSuccessResult(aesEncryptModel);
        }
        #endregion

        #region RSA加解密

        /// <summary>
        /// 生成非对称密钥
        /// </summary>
        /// <returns>返回私钥和公钥元组</returns>
        public (byte[] privateKey, byte[] publicKey) GenerateRSAKey()
        {
            using (RSA rsa = RSA.Create())
            {
                //keysize设置2048 的时候数据长度不可以大于256 如果大于256 我就需要进行分片加解密
                rsa.KeySize = 2048;
                var privateKey = rsa.ExportRSAPrivateKey();
                var publicKey = rsa.ExportRSAPublicKey();
                return (privateKey, publicKey);
            }
        }

        /// <summary>
        /// 非对称加密
        /// </summary>
        /// <param name="encryptContent"></param>
        /// <param name="rsaPublicKey"></param>
        /// <returns></returns>
        public OperateResult<string> RSAEncrypt(string encryptContent, string rsaPublicKey)
        {
            using (RSACryptoServiceProvider rsaEncrypt = new RSACryptoServiceProvider())
            {
                if (string.IsNullOrEmpty(encryptContent))
                {
                    return OperateResult.CreateFailResult<string>("加密的数据不能为空");
                }
                try
                {
                    var publicKey = Convert.FromBase64String(rsaPublicKey);
                    rsaEncrypt.ImportRSAPublicKey(publicKey, out int bytesRead);
                }
                catch (Exception ex)
                {
                    LogBLL.LogError(ex, "密钥导入失败");
                    return OperateResult.CreateFailResult<string>("密钥导入失败");
                }
                var encryptData = rsaEncrypt.Encrypt(Encoding.UTF8.GetBytes(encryptContent), RSAEncryptionPadding.Pkcs1);
                return OperateResult.CreateSuccessResult(Convert.ToBase64String(encryptData));
            }
        }

        /// <summary>
        /// 非对称解密
        /// </summary>
        /// <param name="encryptContent">加密内容</param>
        /// <param name="rsaPrivateKey">RSA私钥</param>
        /// <returns></returns>
        public OperateResult<string> RSADecrypt(string encryptContent, byte[] rsaPrivateKey)
        {
            using (RSACryptoServiceProvider rsaDecrypt = new RSACryptoServiceProvider())
            {
                rsaDecrypt.ImportRSAPrivateKey(rsaPrivateKey, out int bytesRead);
                var decryptData = rsaDecrypt.Decrypt(Convert.FromBase64String(encryptContent), RSAEncryptionPadding.Pkcs1);
                return OperateResult.CreateSuccessResult(Encoding.UTF8.GetString(decryptData));
            }
        }

        /// <summary>
        /// 初始化服务端RSA非对称密钥
        /// </summary>
        /// <param name="rsaPublicKeySavePath">公钥保存路径</param>
        /// <param name="rsaPrivateKeySavePath">私钥保存路径</param>
        public void InitServerRSAKey(string rsaPublicKeySavePath, string rsaPrivateKeySavePath)
        {
            if (File.Exists(rsaPublicKeySavePath) && File.Exists(rsaPrivateKeySavePath))
            {
                return;
            }
            (byte[] privateKey, byte[] publicKey) = GenerateRSAKey();
            var privateKeyProtect = DataProtector.Protect(privateKey);
            var publicKeyProtect = DataProtector.Protect(publicKey);
            File.WriteAllBytes(rsaPublicKeySavePath, publicKeyProtect);
            File.WriteAllBytes(rsaPrivateKeySavePath, privateKeyProtect);
        }

        /// <summary>
        /// 获取RAS非对称公钥
        /// </summary>
        /// <param name="rsaPublicKeySavePath">公钥存储路径</param>
        /// <returns></returns>
        public OperateResult<string> GetRSAPublicKey(string rsaPublicKeySavePath)
        {
            if (!File.Exists(rsaPublicKeySavePath))
            {
                return OperateResult.CreateFailResult<string>("获取公钥失败！");
            }

            var publicKeyProtect = File.ReadAllBytes(rsaPublicKeySavePath);

            var publicKeyUnProtect = DataProtector.Unprotect(publicKeyProtect);
            return OperateResult.CreateSuccessResult(Convert.ToBase64String(publicKeyUnProtect));
        }

        /// <summary>
        /// 获取RSA非对称私钥
        /// </summary>
        /// <param name="rsaPrivateKeySavePath">私钥保存路径</param>
        /// <returns></returns>
        public OperateResult<byte[]> GetRSAPrivateKey(string rsaPrivateKeySavePath)
        {
            if (!File.Exists(rsaPrivateKeySavePath))
            {
                return OperateResult.CreateFailResult<byte[]>("获取私钥失败！");
            }
            var privateKeyProtect = File.ReadAllBytes(rsaPrivateKeySavePath);
            var privateUnProtect = DataProtector.Unprotect(privateKeyProtect);
            return OperateResult.CreateSuccessResult(privateUnProtect);
        }

        /// <summary>
        /// RSA非对称加密签名
        /// </summary>
        /// <param name="hash">哈希值</param>
        /// <param name="rsaPrivateKey">RSA私钥</param>
        /// <returns></returns>
        public OperateResult<string> RSASignData(byte[] hash, byte[] rsaPrivateKey)
        {
            using (RSACryptoServiceProvider rsaSign = new RSACryptoServiceProvider())
            {
                rsaSign.ImportRSAPrivateKey(rsaPrivateKey, out int bytesRead);
                var signData = rsaSign.SignData(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return OperateResult.CreateSuccessResult(Convert.ToBase64String(signData));
            }
        }

        /// <summary>
        /// 获取RSA非对称加密数据哈希值
        /// </summary>
        /// <param name="arrayList">加密数据列表</param>
        /// <returns></returns>
        public OperateResult<byte[]> GetRSAHash(ArrayList arrayList)
        {
            arrayList.Sort(new DictionarySort());
            string raw = "";
            for (int i = 0; i < arrayList.Count; ++i)
            {
                raw += arrayList[i];
            }
            SHA256 sha;
            ASCIIEncoding enc;
            try
            {
                sha = SHA256.Create();
                enc = new ASCIIEncoding();
                byte[] dataToHash = enc.GetBytes(raw);
                byte[] dataHashed = sha.ComputeHash(dataToHash);
                return OperateResult.CreateSuccessResult(dataHashed);
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<byte[]>("生成Hash值失败");
            }
        }


        /// <summary>
        /// RSA非对称加密数据签名验证
        /// </summary>
        /// <param name="hash">哈希值</param>
        /// <param name="signature">数据签名</param>
        /// <param name="rsaPublicKey">RSA公钥</param>
        /// <returns></returns>
        public OperateResult RSAVerifyData(byte[] hash, byte[] signature, string rsaPublicKey)
        {
            using (RSACryptoServiceProvider rsaVerify = new RSACryptoServiceProvider())
            {
                rsaVerify.ImportRSAPublicKey(Convert.FromBase64String(rsaPublicKey), out int bytesRead);
                if (!rsaVerify.VerifyData(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                {
                    return OperateResult.CreateFailResult("签名验证失败");
                }

                return OperateResult.CreateSuccessResult();
            }
        }

        #endregion

        #region 使用DataProtect保存数据
        /// <summary>
        /// 获取加密数据
        /// </summary>
        /// <param name="data">待加密数据</param>
        /// <returns></returns>
        public string ProtectData(string data)
        {
            return DataProtector.Protect(data);
        }

        /// <summary>
        /// 获取解密数据
        /// </summary>
        /// <param name="protectData">加密数据</param>
        /// <returns></returns>
        public string UnprotectData(string protectData)
        {
            return DataProtector.Unprotect(protectData);
        }
        #endregion


        /// <summary>
        /// 将短值由网络字节顺序转换为主机字节顺序。
        /// </summary>
        /// <param name="inval">以网络字节顺序表示的要转换的数字。</param>
        /// <returns></returns>
        public int HostToNetworkOrder(int inval)
        {
            int outval = 0;
            for (int i = 0; i < 4; i++)
            {
                outval = (outval << 8) + ((inval >> (i * 8)) & 255);
            }
            return outval;
        }

        /// <summary>
        /// 解密方法
        /// </summary>
        /// <param name="input">密文</param>
        /// <param name="encodingAESKey">AES对称密钥</param>
        /// <returns></returns>
        public string AESDecrypt(string input, string encodingAESKey, ref string appid)
        {
            byte[] Key;
            Key = Convert.FromBase64String(encodingAESKey + "=");
            byte[] Iv = new byte[16];
            Array.Copy(Key, Iv, 16);
            byte[] btmpMsg = AESDecrypt(input, Iv, Key);

            int len = BitConverter.ToInt32(btmpMsg, 16);
            len = IPAddress.NetworkToHostOrder(len);

            byte[] bMsg = new byte[len];
            byte[] bAppid = new byte[btmpMsg.Length - 20 - len];
            Array.Copy(btmpMsg, 20, bMsg, 0, len);
            Array.Copy(btmpMsg, 20 + len, bAppid, 0, btmpMsg.Length - 20 - len);
            string oriMsg = Encoding.UTF8.GetString(bMsg);
            appid = Encoding.UTF8.GetString(bAppid);
            return oriMsg;
        }

        /// <summary>
        /// AES堆成加密
        /// </summary>
        /// <param name="input">加密字符串</param>
        /// <param name="encodingAESKey">AES对称密钥</param>
        /// <param name="appid">应用主键</param>
        /// <returns></returns>
        public string AESEncrypt(string input, string encodingAESKey, string appid)
        {
            byte[] Key;
            Key = Convert.FromBase64String(encodingAESKey + "=");
            byte[] Iv = new byte[16];
            Array.Copy(Key, Iv, 16);
            string Randcode = CreateRandCode(16);
            byte[] bRand = Encoding.UTF8.GetBytes(Randcode);
            byte[] bAppid = Encoding.UTF8.GetBytes(appid);
            byte[] btmpMsg = Encoding.UTF8.GetBytes(input);
            byte[] bMsgLen = BitConverter.GetBytes(HostToNetworkOrder(btmpMsg.Length));
            byte[] bMsg = new byte[bRand.Length + bMsgLen.Length + bAppid.Length + btmpMsg.Length];

            Array.Copy(bRand, bMsg, bRand.Length);
            Array.Copy(bMsgLen, 0, bMsg, bRand.Length, bMsgLen.Length);
            Array.Copy(btmpMsg, 0, bMsg, bRand.Length + bMsgLen.Length, btmpMsg.Length);
            Array.Copy(bAppid, 0, bMsg, bRand.Length + bMsgLen.Length + btmpMsg.Length, bAppid.Length);

            return AESEncrypt(bMsg, Iv, Key);

        }

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <param name="codeLen">随机数长度</param>
        /// <returns></returns>
        public string CreateRandCode(int codeLen)
        {
            string codeSerial = "2,3,4,5,6,7,a,c,d,e,f,h,i,j,k,m,n,p,r,s,t,A,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,U,V,W,X,Y,Z";
            if (codeLen == 0)
            {
                codeLen = 16;
            }
            string[] arr = codeSerial.Split(',');
            string code = "";
            int randValue = -1;
            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < codeLen; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);
                code += arr[randValue];
            }
            return code;
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="token">token值</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="msgEncrypt">加密消息</param>
        /// <param name="sigture">签名</param>
        /// <returns></returns>
        public OperateResult VerifySignature(string token, string timeStamp, string nonce, string msgEncrypt, string sigture)
        {
            //生成签名
            var genarateSinatureResult = GenarateSinature(token, timeStamp, nonce, msgEncrypt);
            if (!genarateSinatureResult.IsSuccess)
            {
                return genarateSinatureResult;
            }
            string hash = genarateSinatureResult.Data;

            //比较签名是否正确
            if (hash != sigture)
            {
                return OperateResult.CreateFailResult("签名验证失败");
            }


            return OperateResult.CreateSuccessResult();
        }



        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="token">token值</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="msgEncrypt">消息加密</param>
        /// <returns></returns>
        public OperateResult<string> GenarateSinature(string token, string timeStamp, string nonce, string msgEncrypt)
        {
            ArrayList AL = new ArrayList
            {
                token,
                timeStamp,
                nonce,
                msgEncrypt
            };
            AL.Sort(new DictionarySort());
            string raw = "";
            for (int i = 0; i < AL.Count; ++i)
            {
                raw += AL[i];
            }

            SHA1 sha;
            ASCIIEncoding enc;
            string hash = "";
            try
            {
                sha = SHA1.Create();
                enc = new ASCIIEncoding();
                byte[] dataToHash = enc.GetBytes(raw);
                //采用Sha1是生成160位
                byte[] dataHashed = sha.ComputeHash(dataToHash);
                hash = BitConverter.ToString(dataHashed).Replace("-", "");
                hash = hash.ToLower();
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<string>("生成签名失败");
            }
            return OperateResult.CreateSuccessResult<string>(hash);
        }


        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="appId">帐号的appid</param>
        /// <param name="encodingAESKey">AES对称密钥</param>
        /// <param name="token">票据</param>
        /// <param name="msgSignature">签名</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="postData">密文，对应POST请求的数据</param>
        /// <returns></returns>
        public OperateResult<string> AESDecrypt(string appId, string encodingAESKey, string token, string msgSignature, string timeStamp, string nonce, string postData)
        {
            if (encodingAESKey.Length != 43)
            {
                return OperateResult.CreateFailResult<string>("对称密钥不合法");
            }
            XmlDocument doc = new XmlDocument();
            XmlNode root;
            string aesEncrypt;
            try
            {
                doc.LoadXml(postData);
                root = doc.FirstChild;
                aesEncrypt = root["Encrypt"].InnerText;
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<string>("Xml数据解析失败");
            }

            //验证签名
            var verifySignatureResult = this.VerifySignature(token, timeStamp, nonce, aesEncrypt, msgSignature);
            if (!verifySignatureResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<string>(verifySignatureResult);
            }

            //解密
            string appIdDecrypt = "";
            string msgDecrypt = "";
            try
            {
                msgDecrypt = AESDecrypt(aesEncrypt, encodingAESKey, ref appIdDecrypt);
            }
            catch (FormatException)
            {
                return OperateResult.CreateFailResult<string>("Base64解码错误");
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<string>("AES解密失败");
            }
            if (appId != appIdDecrypt)
            {
                return OperateResult.CreateFailResult<string>("AppId验证失败");
            }
            return OperateResult.CreateSuccessResult<string>(msgDecrypt);
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="appId">用户主键</param>
        /// <param name="encodingAESKey">AES对称密钥</param>
        /// <param name="token">票证</param>
        /// <param name="replyMsg">消息内容</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <returns></returns>
        public OperateResult<string> AESEncrypt(string appId, string encodingAESKey, string token, string replyMsg, string timeStamp, string nonce)
        {
            if (encodingAESKey.Length != 43)
            {
                return OperateResult.CreateFailResult<string>("对称密钥不合法");
            }
            string msgEncrypt = "";
            try
            {
                msgEncrypt = AESEncrypt(replyMsg, encodingAESKey, appId);
            }
            catch (Exception)
            {
                return OperateResult.CreateFailResult<string>("AES加密错误");
            }

            //生成签名
            var genarateSinatureResult = GenarateSinature(token, timeStamp, nonce, msgEncrypt);
            if (!genarateSinatureResult.IsSuccess)
            {
                return genarateSinatureResult;
            }
            string msgSigature = genarateSinatureResult.Data;

            string aesEncrypt = "";
            string EncryptLabelHead = "<Encrypt><![CDATA[";
            string EncryptLabelTail = "]]></Encrypt>";
            string MsgSigLabelHead = "<MsgSignature><![CDATA[";
            string MsgSigLabelTail = "]]></MsgSignature>";
            string TimeStampLabelHead = "<TimeStamp><![CDATA[";
            string TimeStampLabelTail = "]]></TimeStamp>";
            string NonceLabelHead = "<Nonce><![CDATA[";
            string NonceLabelTail = "]]></Nonce>";
            aesEncrypt = aesEncrypt + "<xml>" + EncryptLabelHead + msgEncrypt + EncryptLabelTail;
            aesEncrypt = aesEncrypt + MsgSigLabelHead + msgSigature + MsgSigLabelTail;
            aesEncrypt = aesEncrypt + TimeStampLabelHead + timeStamp + TimeStampLabelTail;
            aesEncrypt = aesEncrypt + NonceLabelHead + nonce + NonceLabelTail;
            aesEncrypt += "</xml>";
            return OperateResult.CreateSuccessResult<string>(aesEncrypt);
        }

        /// <summary>
        /// 获取MD5哈希值
        /// </summary>
        /// <param name="hashString">哈希内容</param>
        /// <returns></returns>
        public string GetMD5HashCode(string hashContent)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(hashContent);
                var hash = md5.ComputeHash(buffer);
                return Convert.ToHexString(hash);
            }
        }

        /// <summary>
        /// 获取SHA256哈希值
        /// </summary>
        /// <param name="hashString">哈希内容</param>
        /// <returns></returns>
        public string GetSHA256HashCode(string hashContent)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(hashContent);
                var hash = sha256.ComputeHash(buffer);
                return Convert.ToHexString(hash);
            }
        }

        #region 私有方法

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="input">加密内容</param>
        /// <param name="iv">设置要用于对称算法的初始化向量（IV）</param>
        /// <param name="key">设置用于对称算法的密钥</param>
        /// <returns></returns>
        private string AESEncrypt(string input, byte[] iv, byte[] key)
        {
            var aes = new RijndaelManaged();
            //秘钥的大小，以位为单位
            aes.KeySize = 256;
            //支持的块大小
            aes.BlockSize = 128;
            //填充模式
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.IV = iv;
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(input);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = ms.ToArray();
            }
            string Output = Convert.ToBase64String(xBuff);
            return Output;
        }

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="input">加密内容</param>
        /// <param name="iv">设置要用于对称算法的初始化向量（IV）</param>
        /// <param name="key">设置用于对称算法的密钥</param>
        /// <returns></returns>
        private string AESEncrypt(byte[] input, byte[] iv, byte[] key)
        {
            var aes = new RijndaelManaged();
            //秘钥的大小，以位为单位
            aes.KeySize = 256;
            //支持的块大小
            aes.BlockSize = 128;
            //填充模式
            //aes.Padding = PaddingMode.PKCS7;
            aes.Padding = PaddingMode.None;
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.IV = iv;
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;

            #region 自己进行PKCS7补位，用系统自己带的不行
            byte[] msg = new byte[input.Length + 32 - input.Length % 32];
            Array.Copy(input, msg, input.Length);
            byte[] pad = KCS7Encoder(input.Length);
            Array.Copy(pad, 0, msg, input.Length, pad.Length);
            #endregion

            #region 注释的也是一种方法，效果一样
            //ICryptoTransform transform = aes.CreateEncryptor();
            //byte[] xBuff = transform.TransformFinalBlock(msg, 0, msg.Length);
            #endregion
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    cs.Write(msg, 0, msg.Length);
                }
                xBuff = ms.ToArray();
            }

            string Output = Convert.ToBase64String(xBuff);
            return Output;
        }

        /// <summary>
        /// KCS7编码器
        /// </summary>
        /// <param name="textLength">内容长度</param>
        /// <returns></returns>
        private byte[] KCS7Encoder(int textLength)
        {
            int block_size = 32;
            // 计算需要填充的位数
            int amount_to_pad = block_size - (textLength % block_size);
            if (amount_to_pad == 0)
            {
                amount_to_pad = block_size;
            }
            // 获得补位所用的字符
            char pad_chr = Chr(amount_to_pad);
            string tmp = "";
            for (int index = 0; index < amount_to_pad; index++)
            {
                tmp += pad_chr;
            }
            return Encoding.UTF8.GetBytes(tmp);
        }


        /// <summary>
        /// 将数字转化成ASCII码对应的字符，用于对明文进行补码
        /// </summary>
        /// <param name="a">需要转化的数字</param>
        /// <returns>转化得到的字符</returns>
        private char Chr(int a)
        {
            byte target = (byte)(a & 0xFF);
            return (char)target;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="input">加密内容</param>
        /// <param name="iv">设置要用于对称算法的初始化向量（IV）</param>
        /// <param name="key">设置用于对称算法的密钥</param>
        private byte[] AESDecrypt(string input, byte[] iv, byte[] key)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = key;
            aes.IV = iv;
            var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(input);
                    byte[] msg = new byte[xXml.Length + 32 - xXml.Length % 32];
                    Array.Copy(xXml, msg, xXml.Length);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = Decode2(ms.ToArray());
            }
            return xBuff;
        }


        /// <summary>
        /// 解码2
        /// </summary>
        /// <param name="decrypted">解密内容</param>
        /// <returns></returns>
        private byte[] Decode2(byte[] decrypted)
        {
            int pad = (int)decrypted[decrypted.Length - 1];
            if (pad < 1 || pad > 32)
            {
                pad = 0;
            }
            byte[] res = new byte[decrypted.Length - pad];
            Array.Copy(decrypted, 0, res, 0, decrypted.Length - pad);
            return res;
        }

        #endregion
    }
}
