using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
namespace RS.Commons
{
    /// <summary>
    /// 微信数据AES加解密示例类
    /// </summary>
    public class WechatAESTest
    {
        private readonly ICryptographyService CryptographyService;
        public WechatAESTest(ICryptographyService cryptographyService)
        {
            CryptographyService = cryptographyService;
        }
        public OperateResult Test()
        {
            //公众平台上开发者设置的token, appId, EncodingAESKey
            string appId = "wx5823bf96d3bd56c7";
            string encodingAESKey = "jWmYm7qr5nMoAUwZRjGtBxmz3KA1tkAj3ykkR6q2B2C";
            string token = "QDG6eK";

            /* 1. 对用户回复的数据进行解密。
            * 用户回复消息或者点击事件响应时，企业会收到回调消息，假设企业收到的推送消息：
            * 	POST /cgi-bin/wxpush? msg_signature=477715d11cdb4164915debcba66cb864d751f3e6&timestamp=1409659813&nonce=1372623149 HTTP/1.1
               Host: qy.weixin.qq.com
               Content-Length: 613
            *
            * 	<xml>
                   <ToUserName><![CDATA[wx5823bf96d3bd56c7]]></ToUserName>
                   <Encrypt><![CDATA[RypEvHKD8QQKFhvQ6QleEB4J58tiPdvo+rtK1I9qca6aM/wvqnLSV5zEPeusUiX5L5X/0lWfrf0QADHHhGd3QczcdCUpj911L3vg3W/sYYvuJTs3TUUkSUXxaccAS0qhxchrRYt66wiSpGLYL42aM6A8dTT+6k4aSknmPj48kzJs8qLjvd4Xgpue06DOdnLxAUHzM6+kDZ+HMZfJYuR+LtwGc2hgf5gsijff0ekUNXZiqATP7PF5mZxZ3Izoun1s4zG4LUMnvw2r+KqCKIw+3IQH03v+BCA9nMELNqbSf6tiWSrXJB3LAVGUcallcrw8V2t9EL4EhzJWrQUax5wLVMNS0+rUPA3k22Ncx4XXZS9o0MBH27Bo6BpNelZpS+/uh9KsNlY6bHCmJU9p8g7m3fVKn28H3KDYA5Pl/T8Z1ptDAVe0lXdQ2YoyyH2uyPIGHBZZIs2pDBS8R07+qN+E7Q==]]></Encrypt>
               </xml>
            */
            string reqMsgSig = "477715d11cdb4164915debcba66cb864d751f3e6";
            string reqTimeStamp = "1409659813";
            string reqNonce = "1372623149";
            string reqData = "<xml><ToUserName><![CDATA[wx5823bf96d3bd56c7]]></ToUserName><Encrypt><![CDATA[RypEvHKD8QQKFhvQ6QleEB4J58tiPdvo+rtK1I9qca6aM/wvqnLSV5zEPeusUiX5L5X/0lWfrf0QADHHhGd3QczcdCUpj911L3vg3W/sYYvuJTs3TUUkSUXxaccAS0qhxchrRYt66wiSpGLYL42aM6A8dTT+6k4aSknmPj48kzJs8qLjvd4Xgpue06DOdnLxAUHzM6+kDZ+HMZfJYuR+LtwGc2hgf5gsijff0ekUNXZiqATP7PF5mZxZ3Izoun1s4zG4LUMnvw2r+KqCKIw+3IQH03v+BCA9nMELNqbSf6tiWSrXJB3LAVGUcallcrw8V2t9EL4EhzJWrQUax5wLVMNS0+rUPA3k22Ncx4XXZS9o0MBH27Bo6BpNelZpS+/uh9KsNlY6bHCmJU9p8g7m3fVKn28H3KDYA5Pl/T8Z1ptDAVe0lXdQ2YoyyH2uyPIGHBZZIs2pDBS8R07+qN+E7Q==]]></Encrypt></xml>";

            var aesDecryptResult = CryptographyService.AESDecrypt(appId, encodingAESKey, token, reqMsgSig, reqTimeStamp, reqNonce, reqData);
            if (!aesDecryptResult.IsSuccess)
            {
                return aesDecryptResult;
            }
            //解析之后的明文
            string msgDecrypt = aesDecryptResult.Data;
            Debug.WriteLine(msgDecrypt);

            /*
             * 2. 企业回复用户消息也需要加密和拼接xml字符串。
             * 假设企业需要回复用户的消息为：
             * 		<xml>
             * 		<ToUserName><![CDATA[mycreate]]></ToUserName>
             * 		<FromUserName><![CDATA[wx5823bf96d3bd56c7]]></FromUserName>
             * 		<CreateTime>1348831860</CreateTime>
                    <MsgType><![CDATA[text]]></MsgType>
             *      <Content><![CDATA[this is a test]]></Content>
             *      <MsgId>1234567890123456</MsgId>
             *      </xml>
             * 生成xml格式的加密消息过程为：
             */
            string respData = "<xml><ToUserName><![CDATA[mycreate]]></ToUserName><FromUserName><![CDATA[wx582测试一下中文的情况，消息长度是按字节来算的396d3bd56c7]]></FromUserName><CreateTime>1348831860</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[this is a test]]></Content><MsgId>1234567890123456</MsgId></xml>";
            string sAESEncrypt = ""; //xml格式的密文
            var aesEncryptResult = CryptographyService.AESEncrypt(appId, encodingAESKey, token, respData, reqTimeStamp, reqNonce);
            if (!aesEncryptResult.IsSuccess)
            {
                return aesEncryptResult;
            }
            string aesEncrypt = aesEncryptResult.Data;
            Debug.WriteLine(aesEncrypt);


            /*测试：
             * 将sAESEncrypt解密看看是否是原文
             * */
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sAESEncrypt);
            XmlNode root = doc.FirstChild;
            string sig = root["MsgSignature"].InnerText;
            string enc = root["Encrypt"].InnerText;
            string timestamp = root["TimeStamp"].InnerText;
            string nonce = root["Nonce"].InnerText;

            aesDecryptResult = CryptographyService.AESDecrypt(appId, encodingAESKey, token, sig, timestamp, nonce, sAESEncrypt);
            if (!aesDecryptResult.IsSuccess)
            {
                return aesDecryptResult;
            }
            string stmp = aesDecryptResult.Data;
            Debug.WriteLine(stmp);

            return OperateResult.CreateSuccessResult();
        }
    }
}
