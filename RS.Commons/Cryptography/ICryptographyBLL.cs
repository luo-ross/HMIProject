using RS.Models;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace RS.Commons
{
    /// <summary>
    /// 数据加解密服务接口
    /// </summary>
    public interface ICryptographyBLL
    {
        #region AES加解密
        /// <summary>
        /// 获取会话数据
        /// </summary>
        /// <returns></returns>
        OperateResult<SessionModel> GetSessionModel();

        /// <summary>
        /// AES对称解密
        /// </summary>
        /// <typeparam name="TResult">解密实体类型</typeparam>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        OperateResult<TResult> AESDecrypt<TResult>(AESEncryptModel aesEncryptModel);

        /// <summary>
        /// AES对称数据解密
        /// </summary>
        /// <typeparam name="TResult">返回实体类型</typeparam>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionModel">会话实体类</param>
        /// <returns></returns>
        OperateResult<TResult> AESDecrypt<TResult>(AESEncryptModel aesEncryptModel, SessionModel sessionModel);

        /// <summary>
        /// 生成AES对称密钥
        /// </summary>
        /// <returns></returns>
        string GenerateAESKey();

        /// <summary>
        /// AES对称加密
        /// </summary>
        /// <typeparam name="T">待加密数据类型</typeparam>
        /// <param name="encryptModelShould">待加密数据</param>
        /// <returns></returns>
        OperateResult<AESEncryptModel> AESEncrypt<T>(T encryptModelShould);

        /// <summary>
        /// AES对称加密
        /// </summary>
        /// <typeparam name="T">待加密数据类型</typeparam>
        /// <param name="encryptModelShould">待加密数据</param>
        /// <param name="sessionModel">会话实体</param>
        /// <returns></returns>
        OperateResult<AESEncryptModel> AESEncrypt<T>(T encryptModelShould, SessionModel sessionModel);
        #endregion

        #region RSA加解密
        /// <summary>
        /// 生成非对称密钥
        /// </summary>
        /// <returns>返回私钥和公钥元组</returns>
        (byte[] privateKey, byte[] publicKey) GenerateRSAKey();

        /// <summary>
        /// 非对称加密
        /// </summary>
        /// <param name="encryptContent"></param>
        /// <param name="rsaPublicKey"></param>
        /// <returns></returns>
        OperateResult<string> RSAEncrypt(string encryptContent, string rsaPublicKey);

        /// <summary>
        /// 非对称解密
        /// </summary>
        /// <param name="encryptContent">加密内容</param>
        /// <param name="rsaPrivateKey">RSA私钥</param>
        /// <returns></returns>
        OperateResult<string> RSADecrypt(string encryptContent, byte[] rsaPrivateKey);

        /// <summary>
        /// 初始化服务端RSA非对称密钥
        /// </summary>
        /// <param name="rsaPublicKeySavePath">公钥保存路径</param>
        /// <param name="rsaPrivateKeySavePath">私钥保存路径</param>
        void InitServerRSAKey(string rsaPublicKeySavePath, string rsaPrivateKeySavePath);

        /// <summary>
        /// 获取RAS非对称公钥
        /// </summary>
        /// <param name="rsaPublicKeySavePath">公钥存储路径</param>
        /// <returns></returns>
        OperateResult<string> GetRSAPublicKey(string rsaPublicKeySavePath);

        /// <summary>
        /// 获取RSA非对称私钥
        /// </summary>
        /// <param name="rsaPrivateKeySavePath">私钥保存路径</param>
        /// <returns></returns>
        OperateResult<byte[]> GetRSAPrivateKey(string rsaPrivateKeySavePath);

        /// <summary>
        /// RSA非对称加密签名
        /// </summary>
        /// <param name="hash">哈希值</param>
        /// <param name="rsaPrivateKey">RSA私钥</param>
        /// <returns></returns>
        OperateResult<string> RSASignData(byte[] hash, byte[] rsaPrivateKey);

        /// <summary>
        /// 获取RSA非对称加密数据哈希值
        /// </summary>
        /// <param name="arrayList">加密数据列表</param>
        /// <returns></returns>
        OperateResult<byte[]> GetRSAHash(ArrayList arrayList);

        /// <summary>
        /// RSA非对称加密数据签名验证
        /// </summary>
        /// <param name="hash">哈希值</param>
        /// <param name="signature">数据签名</param>
        /// <param name="rsaPublicKey">RSA公钥</param>
        /// <returns></returns>
        OperateResult RSAVerifyData(byte[] hash, byte[] signature, string rsaSigningPublicKey);
        #endregion

        #region 使用DataProtect保存数据
        /// <summary>
        /// 获取加密数据
        /// </summary>
        /// <param name="data">待加密数据</param>
        /// <returns></returns>
        string ProtectData(string data);

        /// <summary>
        /// 获取解密数据
        /// </summary>
        /// <param name="protectData">加密数据</param>
        /// <returns></returns>
        string UnprotectData(string protectData);
        #endregion

        /// <summary>
        /// 将短值由网络字节顺序转换为主机字节顺序。
        /// </summary>
        /// <param name="inval">以网络字节顺序表示的要转换的数字。</param>
        /// <returns></returns>
        int HostToNetworkOrder(int inval);

        /// <summary>
        /// 解密方法
        /// </summary>
        /// <param name="input">密文</param>
        /// <param name="encodingAESKey">AES对称密钥</param>
        /// <returns></returns>
        string AESDecrypt(string input, string encodingAESKey, ref string appid);

        /// <summary>
        /// AES堆成加密
        /// </summary>
        /// <param name="input">加密字符串</param>
        /// <param name="encodingAESKey">AES对称密钥</param>
        /// <param name="appid">应用主键</param>
        /// <returns></returns>
        string AESEncrypt(string input, string encodingAESKey, string appid);

        /// <summary>
        /// 创建随机数
        /// </summary>
        /// <param name="codeLen">随机数长度</param>
        /// <returns></returns>
        string CreateRandCode(int codeLen);

        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="token">token值</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="msgEncrypt">加密消息</param>
        /// <param name="sigture">签名</param>
        /// <returns></returns>
        public OperateResult VerifySignature(string token, string timeStamp, string nonce, string msgEncrypt, string sigture);

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="token">token值</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="msgEncrypt">消息加密</param>
        /// <returns></returns>
        OperateResult<string> GenarateSinature(string token, string timeStamp, string nonce, string msgEncrypt);

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
        OperateResult<string> AESDecrypt(string appId, string encodingAESKey, string token, string msgSignature, string timeStamp, string nonce, string postData);

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
        OperateResult<string> AESEncrypt(string appId, string encodingAESKey, string token, string replyMsg, string timeStamp, string nonce);

        /// <summary>
        /// 获取MD5哈希值
        /// </summary>
        /// <param name="hashString">哈希内容</param>
        /// <returns></returns>
        string GetMD5HashCode(string hashContent);

        /// <summary>
        /// 获取SHA256哈希值
        /// </summary>
        /// <param name="hashString">哈希内容</param>
        /// <returns></returns>
        string GetSHA256HashCode(string hashContent);
    }
}
