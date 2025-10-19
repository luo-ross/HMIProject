//用户机密模版
{
  "ConnectionStrings": {
    "RSAppSqlSever": "Data Source=.;Initial Catalog=RSApplication;User ID=sa;Encrypt=True;Trust Server Certificate=True;Password=123456"
  },
  "AllowedClients": {
    "Origins": [
      "http://localhost:7001",
      "http://localhost:54293"
    ]
  },
  "ApplicationDiscriminator": "145164B9-11BA-4127-A678-17764D582031",
  "EmailService": {
    "Server": "Tencent",
    "Host": "smtp.qq.com",
    "Port": "587",
    "UserName": "1845960295@qq.com",
    "Password": "eyimt123laagdjci",
  },
  "GlobalRSAEncryptPrivateKeyFileName": "key-FFD06E4F-0219-4C2F-AABC-0F97BAFF4792.bin",
  "GlobalRSAEncryptPublicKeyFileName": "key-C84193C7-CE9C-4898-86A5-F6B624440E88.bin",
  "GlobalRSASignPrivateKeyFileName": "key-7948DE50-2B7F-4E39-9382-8CA7B8125625.bin",
  "GlobalRSASignPublicKeyFileName": "key-555C1014-E849-4456-98FC-083307422D01.bin",
  "RSAppRedis": {
    "Host": "127.0.0.1",
    "Password": "123456",
    "Port": "6379"
  },
  "SMSService": {
    "Server": "Aliyun",
    "AccessKeyId": "123456",
    "AccessKeySecret": "123123123"
  },
  "JWTConfig": {
    "Audiences": [
      "Andriod",
      "DingTalk",
      "IOS",
      "TikTalk",
      "WeChat",
      "Windows",
      "Web"
    ],
    "Issuer": "http://wpf.mycompany.com/authapi",
    "SecurityKey": "http://localhost:7001"
  },
}