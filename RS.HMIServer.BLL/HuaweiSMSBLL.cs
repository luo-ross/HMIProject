using Microsoft.Extensions.Configuration;
using RS.HMIServer.IBLL;
using RS.Commons;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RS.HMIServer.BLL
{

    /// <summary>
    /// 数据签名
    /// </summary>
    public partial class Signer
    {

        private static char IntToHex(int n)
        {
            if (n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'A');
        }

        private static bool IsUrlSafeChar(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '~':
                    return true;
            }
            return false;
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cUnsafe == 0)
            {
                // DevDiv 912606: respect "offset" and "count"
                if (0 == offset && bytes.Length == count)
                {
                    return bytes;
                }
                else
                {
                    var subarray = new byte[count];
                    Buffer.BlockCopy(bytes, offset, subarray, 0, count);
                    return subarray;
                }
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        private static string UrlEncode(string value)
        {
            if (value == null) {
                return null;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(UrlEncode(bytes, 0, bytes.Length));
        }

    }

    /// <summary>
    /// Http请求
    /// </summary>
    public class HttpRequest
    {
        public string method;
        public string host; /*   http://example.com  */
        public string uri = "/";  /*   /request/uri      */
        public Dictionary<string, List<string>> query = new Dictionary<string, List<string>>();
        public WebHeaderCollection headers = new WebHeaderCollection();
        public string body = "";
        public string canonicalRequest;
        public string stringToSign;
        public HttpRequest(string method = "GET", Uri url = null, WebHeaderCollection headers = null, string body = null)
        {
            if (method != null)
            {
                this.method = method;
            }
            if (url != null)
            {
                host = url.Scheme + "://" + url.Host + ":" + url.Port;
                uri = url.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);
                query = new Dictionary<string, List<string>>();
                if (url.Query.Length > 1)
                {
                    foreach (var kv in url.Query.Substring(1).Split('&'))
                    {
                        string[] spl = kv.Split(new char[] { '=' }, 2);
                        string key = Uri.UnescapeDataString(spl[0]);
                        string value = "";
                        if (spl.Length > 1)
                        {
                            value = Uri.UnescapeDataString(spl[1]);
                        }
                        if (query.ContainsKey(key))
                        {
                            query[key].Add(value);
                        }
                        else
                        {
                            query[key] = new List<string> { value };
                        }
                    }
                }
            }
            if (headers != null)
            {
                this.headers = headers;
            }
            if (body != null)
            {
                this.body = body;
            }
        }
    }

    /// <summary>
    /// 数据签名
    /// </summary>
    public partial class Signer
    {
        const string BasicDateFormat = "yyyyMMddTHHmmssZ";
        const string Algorithm = "SDK-HMAC-SHA256";
        const string HeaderXDate = "X-Sdk-Date";
        const string HeaderHost = "host";
        const string HeaderAuthorization = "Authorization";
        const string HeaderContentSha256 = "X-Sdk-Content-Sha256";
        readonly HashSet<string> unsignedHeaders = new HashSet<string> { "content-type" };

        private string key;
        private string secret;

        public string AppKey
        {
            get => key;
            set => key = value;
        }
        public string AppSecret
        {
            get => secret;
            set => secret = value;
        }
        public string Key
        {
            get => key;
            set => key = value;
        }
        public string Secret
        {
            get => secret;
            set => secret = value;
        }

        byte[] hmacsha256(byte[] keyByte, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                return hmacsha256.ComputeHash(messageBytes);
            }
        }

        // Build a CanonicalRequest from a regular request string
        //
        // CanonicalRequest =
        //  HTTPRequestMethod + '\n' +
        //  CanonicalURI + '\n' +
        //  CanonicalQueryString + '\n' +
        //  CanonicalHeaders + '\n' +
        //  SignedHeaders + '\n' +
        //  HexEncode(Hash(RequestPayload))
        string CanonicalRequest(HttpRequest r, List<string> signedHeaders)
        {
            string hexencode;
            if (r.headers.Get(HeaderContentSha256) != null)
            {
                hexencode = r.headers.Get(HeaderContentSha256);
            }
            else
            {
                var data = Encoding.UTF8.GetBytes(r.body);
                hexencode = HexEncodeSHA256Hash(data);
            }
            return string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}", r.method, CanonicalURI(r), CanonicalQueryString(r), CanonicalHeaders(r, signedHeaders), string.Join(";", signedHeaders), hexencode);
        }
        string CanonicalURI(HttpRequest r)
        {
            var pattens = r.uri.Split('/');
            List<string> uri = new List<string>();
            foreach (var v in pattens)
            {
                uri.Add(UrlEncode(v));
            }
            var urlpath = string.Join("/", uri);
            if (urlpath[urlpath.Length - 1] != '/')
            {
                urlpath = urlpath + "/"; // always end with /
            }
            //r.uri = urlpath;
            return urlpath;
        }
        string CanonicalQueryString(HttpRequest r)
        {
            List<string> keys = new List<string>();
            foreach (var pair in r.query)
            {
                keys.Add(pair.Key);
            }
            keys.Sort(String.CompareOrdinal);
            List<string> a = new List<string>();
            foreach (var key in keys)
            {
                string k = UrlEncode(key);
                List<string> values = r.query[key];
                values.Sort(String.CompareOrdinal);
                foreach (var value in values)
                {
                    string kv = k + "=" + UrlEncode(value);
                    a.Add(kv);
                }
            }
            return string.Join("&", a);
        }
        string CanonicalHeaders(HttpRequest r, List<string> signedHeaders)
        {
            List<string> a = new List<string>();
            foreach (string key in signedHeaders)
            {
                var values = new List<string>(r.headers.GetValues(key));
                values.Sort(String.CompareOrdinal);
                foreach (var value in values)
                {
                    a.Add(key + ":" + value.Trim());
                    r.headers.Set(key, Encoding.GetEncoding("iso-8859-1").GetString(Encoding.UTF8.GetBytes(value)));
                }
            }
            return string.Join("\n", a) + "\n";
        }
        List<string> SignedHeaders(HttpRequest r)
        {
            List<string> a = new List<string>();
            foreach (string key in r.headers.AllKeys)
            {
                string keyLower = key.ToLower();
                if (!unsignedHeaders.Contains(keyLower))
                {
                    a.Add(key.ToLower());
                }
            }
            a.Sort(String.CompareOrdinal);
            return a;
        }

        static char GetHexValue(int i)
        {
            if (i < 10)
            {
                return (char)(i + '0');
            }
            return (char)(i - 10 + 'a');
        }
        public static string toHexString(byte[] value)
        {
            int num = value.Length * 2;
            char[] array = new char[num];
            int num2 = 0;
            for (int i = 0; i < num; i += 2)
            {
                byte b = value[num2++];
                array[i] = GetHexValue(b / 16);
                array[i + 1] = GetHexValue(b % 16);
            }
            return new string(array, 0, num);
        }
        // Create a "String to Sign".
        string StringToSign(string canonicalRequest, DateTime t)
        {
            SHA256 sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest));
            sha256.Clear();
            return string.Format("{0}\n{1}\n{2}", Algorithm, t.ToUniversalTime().ToString(BasicDateFormat), toHexString(bytes));
        }


        // Create the HWS Signature.
        string SignStringToSign(string stringToSign, byte[] signingKey)
        {
            byte[] hm = hmacsha256(signingKey, stringToSign);
            return toHexString(hm);
        }
        // HexEncodeSHA256Hash returns hexcode of sha256
        public static string HexEncodeSHA256Hash(byte[] body)
        {
            SHA256 sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(body);
            sha256.Clear();
            return toHexString(bytes);
        }
        public static string HexEncodeSHA256HashFile(string fname)
        {
            SHA256 sha256 = SHA256.Create();
            using (var fs = new FileStream(fname, FileMode.Open))
            {
                var bytes = sha256.ComputeHash(fs);
                sha256.Clear();
                return toHexString(bytes);
            }
        }
        // Get the finalized value for the "Authorization" header. The signature parameter is the output from SignStringToSign
        string AuthHeaderValue(string signature, List<string> signedHeaders)
        {
            return string.Format("{0} Access={1}, SignedHeaders={2}, Signature={3}", Algorithm, key, string.Join(";", signedHeaders), signature);
        }

        public bool Verify(HttpRequest r, string signature)
        {
            if (r.method != "POST" && r.method != "PATCH" && r.method != "PUT")
            {
                r.body = "";
            }
            var time = r.headers.GetValues(HeaderXDate);
            if (time == null)
            {
                return false;
            }
            DateTime t = DateTime.ParseExact(time[0], BasicDateFormat, CultureInfo.CurrentCulture);
            var signedHeaders = SignedHeaders(r);
            var canonicalRequest = CanonicalRequest(r, signedHeaders);
            var stringToSign = StringToSign(canonicalRequest, t);
            return signature == SignStringToSign(stringToSign, Encoding.UTF8.GetBytes(secret));
        }

        // SignRequest set Authorization header
        public HttpWebRequest Sign(HttpRequest r)
        {
            if (r.method != "POST" && r.method != "PATCH" && r.method != "PUT")
            {
                r.body = "";
            }
            var time = r.headers.GetValues(HeaderXDate);
            DateTime t;
            if (time == null)
            {
                t = DateTime.Now;
                r.headers.Add(HeaderXDate, t.ToUniversalTime().ToString(BasicDateFormat));
            }
            else
            {
                t = DateTime.ParseExact(time[0], BasicDateFormat, CultureInfo.CurrentCulture);
            }
            var queryString = CanonicalQueryString(r);
            if (queryString != "")
            {
                queryString = "?" + queryString;
            }
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(r.host + r.uri + queryString);
            string host = null;
            if (r.headers.GetValues(HeaderHost) != null)
            {
                host = r.headers.GetValues(HeaderHost)[0];
                req.Host = host;
            }
            else
            {
                host = req.Host;
            }

            r.headers.Set("host", host);
            var signedHeaders = SignedHeaders(r);
            var canonicalRequest = CanonicalRequest(r, signedHeaders);
            var stringToSign = StringToSign(canonicalRequest, t);
            var signature = SignStringToSign(stringToSign, Encoding.UTF8.GetBytes(secret));
            var authValue = AuthHeaderValue(signature, signedHeaders);
            r.headers.Set(HeaderAuthorization, authValue);
            req.Method = r.method;
            r.headers.Remove("host");
            string[] reservedHeaders = new String[]
            {
                "content-type","accept","date","if-modified-since","referer","user-agent",
            };
            Dictionary<string, string> savedHeaders = new Dictionary<string, string>();
            foreach (string header in reservedHeaders)
            {
                if (r.headers.GetValues(header) != null)
                {
                    savedHeaders[header] = r.headers.GetValues(header)[0];
                    r.headers.Remove(header);
                }
            }
            req.Headers = r.headers;
            if (savedHeaders.ContainsKey("content-type"))
            {
                req.ContentType = savedHeaders["content-type"];
            }
            if (savedHeaders.ContainsKey("accept"))
            {
                req.Accept = savedHeaders["accept"];
            }
            if (savedHeaders.ContainsKey("date"))
            {
                req.Date = Convert.ToDateTime(savedHeaders["date"]);
            }
            if (savedHeaders.ContainsKey("if-modified-since"))
            {
                req.IfModifiedSince = Convert.ToDateTime(savedHeaders["if-modified-since"]);
            }
            if (savedHeaders.ContainsKey("referer"))
            {
                req.Referer = savedHeaders["referer"];
            }
            if (savedHeaders.ContainsKey("user-agent"))
            {
                req.UserAgent = savedHeaders["user-agent"];
            }
            return req;
        }

        public HttpRequestMessage SignHttp(HttpRequest r)
        {
            var queryString = CanonicalQueryString(r);
            if (queryString != "")
            {
                queryString = "?" + queryString;
            }
            HttpRequestMessage req = new HttpRequestMessage(new HttpMethod(r.method), r.host + r.uri + queryString);
            if (r.method != "POST" && r.method != "PATCH" && r.method != "PUT")
            {
                r.body = "";
            }
            else
            {
                req.Content = new StringContent(r.body);
            }
            var time = r.headers.GetValues(HeaderXDate);
            DateTime t;
            if (time == null)
            {
                t = DateTime.Now;
                r.headers.Add(HeaderXDate, t.ToUniversalTime().ToString(BasicDateFormat));
            }
            else
            {
                t = DateTime.ParseExact(time[0], BasicDateFormat, CultureInfo.CurrentCulture);
            }
            string host = null;
            if (r.headers.GetValues(HeaderHost) != null)
            {
                host = r.headers.GetValues(HeaderHost)[0];
                req.Headers.Host = host;
            }
            else
            {
                host = req.RequestUri.Host;
            }

            r.headers.Set("host", host);
            var signedHeaders = SignedHeaders(r);
            var canonicalRequest = CanonicalRequest(r, signedHeaders);
            r.canonicalRequest = canonicalRequest;
            var stringToSign = StringToSign(canonicalRequest, t);
            r.stringToSign = stringToSign;
            var signature = SignStringToSign(stringToSign, Encoding.UTF8.GetBytes(secret));
            var authValue = AuthHeaderValue(signature, signedHeaders);
            r.headers.Set(HeaderAuthorization, authValue);
            r.headers.Remove("host");
            foreach (string key in r.headers.AllKeys)
            {
                req.Headers.TryAddWithoutValidation(key, r.headers[key]);
            }

            return req;
        }

    }

    /// <summary>
    /// 阿里云短信发送服务
    /// </summary>
    internal class HuaweiSMSBLL : ISMSBLL
    {
        private readonly IConfiguration Configuration;
        public HuaweiSMSBLL(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 发送注册短信验证码
        /// </summary>
        /// <param name="countryCode">国家区号</param>
        /// <param name="phone">电话号码</param>
        /// <param name="verify">验证码</param>
        /// <returns></returns>
        public async Task<OperateResult> SendRegisterVerifyAsync(string countryCode, string phone, int verify)
        {
            string accessKeyId = Configuration["SMSConfig:AccessKeyId"];
            string accessKeySecret = Configuration["SMSConfig:AccessKeySecret"];

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //必填,请参考"开发准备"获取如下数据,替换为实际值
            string apiAddress = "https://smsapi.cn-north-4.myhuaweicloud.com:443/sms/batchSendSms/v1"; //APP接入地址(在控制台"应用管理"页面获取)+接口访问URI
                                                                                                       // 认证用的appKey和appSecret硬编码到代码中或者明文存储都有很大的安全风险，建议在配置文件或者环境变量中密文存放，使用时解密，确保安全；

            //国内短信无需填写该项；国际/港澳台短信已申请独立 SenderId 需要填写该字段，默认使用公共 SenderId，无需填写该字段
            string sender = "csms12345678"; //国内短信签名通道号
            string templateId = "2123123123123"; //模板ID

            //条件必填,国内短信关注,当templateId指定的模板类型为通用模板时生效且必填,必须是已审核通过的,与模板类型一致的签名名称

            //string signature = "华为云短信测试"; //签名名称

            //必填,全局号码格式(包含国家码),示例:+86151****6789,多个号码之间用英文逗号分隔
            string receiver = $"+{countryCode}{phone}"; //短信接收人号码

            //选填,短信状态报告接收地址,推荐使用域名,为空或者不填表示不接收状态报告
            string statusCallBack = "";

            //使用国内短信通用模板时,必须填写签名名称
            string signature = "";

            /*
             * 选填,使用无变量模板时请赋空值 string templateParas = "";
             * 单变量模板示例:模板内容为"您的验证码是${1}"时,templateParas可填写为"[\"369751\"]"
             * 双变量模板示例:模板内容为"您有${1}件快递请到${2}领取"时,templateParas可填写为"[\"3\",\"人民公园正门\"]"
             * 模板中的每个变量都必须赋值，且取值不能为空
             * 查看更多模板规范和变量规范:产品介绍>短信模板须知和短信变量须知
             */
            string templateParas = $"[\"{verify}\"]"; //模板变量，此处以单变量验证码短信为例，请客户自行生成6位验证码，并定义为字符串类型，以杜绝首位0丢失的问题（例如：002569变成了2569）。

            Signer signer = new Signer();
            signer.Key = accessKeyId;
            signer.Secret = accessKeySecret;

            HttpRequest r = new HttpRequest("POST", new Uri(apiAddress)); //APP接入地址(在控制台"应用管理"页面获取)+接口访问URI

            //请求Body
            var body = new Dictionary<string, string>() {
                {"from", sender},
                {"to", receiver},
                {"templateId", templateId},
                {"templateParas", templateParas},
                {"statusCallback", statusCallBack},
                {"signature", signature} 
            };

            r.body = new FormUrlEncodedContent(body).ReadAsStringAsync().Result;
            r.headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpWebRequest req = signer.Sign(r);

            //Console.WriteLine(req.Headers.GetValues("x-sdk-date")[0]);
            //Console.WriteLine(string.Join(", ", req.Headers.GetValues("authorization")));
            //Console.WriteLine("body: " + r.body);

            // 不校验证书
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            //try
            //{
            //    var writer = new StreamWriter(req.GetRequestStream());
            //    writer.Write(r.body);
            //    writer.Flush();
            //    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            //    var reader = new StreamReader(resp.GetResponseStream());
            //    Console.WriteLine(reader.ReadToEnd());
            //}
            //catch (WebException e)
            //{
            //    HttpWebResponse resp = (HttpWebResponse)e.Response;
            //    if (resp != null)
            //    {
            //        Console.WriteLine((int)resp.StatusCode + " " + resp.StatusDescription);
            //        var reader = new StreamReader(resp.GetResponseStream());
            //        Console.WriteLine(reader.ReadToEnd());
            //    }
            //    else
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //}

            var writer = new StreamWriter(req.GetRequestStream());
            writer.Write(r.body);
            writer.Flush();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var reader = new StreamReader(resp.GetResponseStream());

            //这里我们可以根据实际调试的结果返回记录日志 这里没有去注册阿里云短信实际
            //调用是不知道返回是什么 需要具体测试
            var rep = reader.ReadToEnd();

            return OperateResult.CreateSuccessResult();
        }
    }
}
