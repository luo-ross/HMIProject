import {
  CommonUtils,
  SessionRequestModel,
  type Data,
  type GernerateKeyResult,
  type OperateResult
} from '../scripts/Utils'

import WebApi from '../scripts/AxiosConfig'


/**
 * 签名类
 */
export class SignModel {
  /**
   * 时间戳
   */
  public TimeStamp: string;

  /**
   * 随机数
   */
  public Nonce: string;

  /**
   * 签名
   */
  public MsgSignature: string;

  /**
   * 构造函数
   * @param timeStamp 时间戳
   * @param nonce 随机数
   * @param msgSignature 签名
   */
  constructor(timeStamp: string = "", nonce: string = "", msgSignature: string = "") {
    this.TimeStamp = timeStamp;
    this.Nonce = nonce;
    this.MsgSignature = msgSignature;
  }
}

/**
 * AES加密类
 */
export class AESEncryptModel extends SignModel {
  /**
   * 加密后的数据
   */
  public Encrypt: string;

  /**
   * 构造函数
   * @param timeStamp 时间戳
   * @param nonce 随机数
   * @param msgSignature 签名
   * @param encrypt 加密后的数据
   */
  constructor(timeStamp: string = "", nonce: string = "", msgSignature: string = "", encrypt: string = "") {
    super(timeStamp, nonce, msgSignature);
    this.Encrypt = encrypt;
  }
}

/**
 * 会话类
 */
export class SessionModel {
  /**
   * 加密后的AES 秘钥
   */
  public AesKey: string;

  /**
   * 会话ID 
   */
  public AppId: string;

  /**
   * 回传给客户端的Token
   */
  public Token: string;

  /**
   * 构造函数
   * @param aesKey 加密后的AES 秘钥
   * @param appId 会话ID
   * @param token 回传给客户端的Token
   */
  constructor(aesKey: string = "", appId: string = "", token: string = "") {
    this.AesKey = aesKey;
    this.AppId = appId;
    this.Token = token;
  }
}

/**
 * 邮箱注册信息类
 */
export class EmailRegisterPostModel {
  /**
   * 邮箱地址
   */
  public Email: string;

  /**
   * 密码
   */
  public Password: string | null;

  /**
   * 构造函数
   * @param email 邮箱地址
   * @param password 密码
   */
  constructor(email: string = "", password: string = "") {
    this.Email = email;
    this.Password = password;
  }
}



export class Cryptography {

  public GetRSASignKeyResult: GernerateKeyResult | null = null;
  public GetRSAEncryptKeyResult: GernerateKeyResult | null = null;
  public AESKeyDecryptResult: string | null = null;
  public AppIdDecryptResult: string | null = null;
  public CommonUtils;
  private static Instance: Cryptography;
  private Cryptography: Cryptography | null = null;

  private constructor() {
    this.CommonUtils = new CommonUtils();
    this.InitDefaultKeys();
  }

  public static GetInstance(): Cryptography {
    if (!Cryptography.Instance) {
      Cryptography.Instance = new Cryptography();
    }
    return Cryptography.Instance;
  }


  /**
   * 获取SHA256哈希值
   * @param hashContent 哈希内容
   * @returns 返回哈希值的十六进制字符串
   */
  public async GetSHA256HashCode(hashContent: string): Promise<OperateResult<string>> {
    try {
      // 将字符串转换为 UTF-8 编码的字节数组
      const encoder = new TextEncoder();
      const data = encoder.encode(hashContent);

      // 计算 SHA-256 哈希值
      const hashBuffer = await crypto.subtle.digest('SHA-256', data);

      // 将哈希值转换为十六进制字符串
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

      return {
        IsSuccess: true,
        Data: hashHex,
        Message: "Success"
      };
    } catch (error) {
      return {
        IsSuccess: false,
        Data: null,
        Message: `计算SHA256哈希值失败: ${error}`
      };
    }
  }

  // 获取用户名
  private async InitDefaultKeys(): Promise<void> {

    this.GetRSASignKeyResult = await this.GetRSAKeyAsync("Sign");
    //如果缓存里没有，则从新创建
    if (!this.GetRSASignKeyResult.IsSuccess) {
      //创建签名密钥
      this.GetRSASignKeyResult = await this.GenerateRSAKeysAsync("Sign", "RSASSA-PKCS1-v1_5", ["sign", "verify"]);
      if (!this.GetRSASignKeyResult.IsSuccess) {
        this.CommonUtils.ShowDangerMsg(this.GetRSASignKeyResult.Message);
        return;
      }
    }

    this.GetRSAEncryptKeyResult = await this.GetRSAKeyAsync("Encrypt");
    if (!this.GetRSAEncryptKeyResult.IsSuccess) {
      //创建加密密钥
      this.GetRSAEncryptKeyResult = await this.GenerateRSAKeysAsync("Encrypt", "RSA-OAEP", ["encrypt", "decrypt"]);
      if (!this.GetRSAEncryptKeyResult.IsSuccess) {
        this.CommonUtils.ShowDangerMsg(this.GetRSAEncryptKeyResult.Message);
        return;
      }
    }

    // 将Date对象转换为Unix时间戳（毫秒）
    const timestamp = new Date().getTime();
    //创建会话请求
    const sessionRequestModel = new SessionRequestModel();

    sessionRequestModel.RSAEncryptPublicKey = this.GetRSASignKeyResult.PublicKey;
    sessionRequestModel.RSASignPublicKey = this.GetRSASignKeyResult.PublicKey;
    sessionRequestModel.RSAEncryptPublicKey = this.GetRSAEncryptKeyResult.PublicKey;
    sessionRequestModel.Nonce = this.CommonUtils.CreateRandCode(10).toString();
    sessionRequestModel.TimeStamp = timestamp.toString();
    sessionRequestModel.AudienceType = "WebAudience";

    const arrayList = [
      sessionRequestModel.RSASignPublicKey,
      sessionRequestModel.RSAEncryptPublicKey,
      sessionRequestModel.Nonce,
      sessionRequestModel.TimeStamp,
    ];

    //获取会话的Hash数据
    const getRSAHashResult = await this.GetRSAHashAsync(arrayList);
    if (!getRSAHashResult.IsSuccess) {
      this.CommonUtils.ShowDangerMsg(getRSAHashResult.Message);
      return;
    }

    const rsaSignDataResult = await this.RSASignDataAsync(getRSAHashResult.Data, this.GetRSASignKeyResult?.PrivateKey);
    if (!rsaSignDataResult.IsSuccess) {
      this.CommonUtils.ShowDangerMsg(rsaSignDataResult.Message);
      return;
    }
    sessionRequestModel.MsgSignature = rsaSignDataResult.Data;

    //const result = await this.CommonUtils.AjaxPost("/api/v1/General/GetSessionModel", sessionRequestModel);
    //await fetch("http://localhost:54293/api/v1/General/GetSessionModel", {
    //  method: 'POST',
    //  headers: {
    //    'Content-Type': 'application/json'
    //  },
    //  body: JSON.stringify(SessionRequestModel)
    //});


    const result = await WebApi.post<SessionRequestModel, OperateResult<Data>>('/api/v1/General/GetSessionModel', sessionRequestModel);


    const getSessionModelResult = await this.GetSessionModel(result);

    if (!getSessionModelResult.IsSuccess) {
      this.CommonUtils.ShowDangerMsg(getSessionModelResult.Message);
      return;
    }

    return;
  }

  public async GetRSAKeyAsync(keyName: string): Promise<GernerateKeyResult> {

    try {
      // 检查参数
      if (!keyName || typeof keyName !== 'string') {
        return {
          IsSuccess: false,
          PublicKey: null,
          PrivateKey: null,
          Message: "密钥名称不能为空"
        };
      }

      // 存储到sessionStorage
      const rsaPublicKey = sessionStorage.getItem(`RSA${keyName}PublicKey`);
      const rsaPrivateKey = sessionStorage.getItem(`RSA${keyName}PrivateKey`);

      if (!rsaPublicKey || !rsaPrivateKey) {
        return {
          IsSuccess: false,
          PublicKey: null,
          PrivateKey: null,
          Message: `${keyName}的密钥不存在，请先生成密钥对`
        };
      }

      return {
        IsSuccess: true,
        PublicKey: rsaPublicKey,
        PrivateKey: rsaPrivateKey,
        Message: `${keyName}的密钥获取成功`
      };


    } catch {
      return {
        IsSuccess: false,
        PublicKey: null,
        PrivateKey: null,
        Message: `获取RSA密钥失败`
      };
    }
  }

  public async GetSessionModel(operateResult: OperateResult<Data>)
    : Promise<OperateResult<string>> {
    if (!operateResult.IsSuccess) {
      return {
        IsSuccess: true,
        Data: null,
        Message: operateResult.Message
      };
    }

    const data = operateResult.Data;
    const sessionModel = data?.SessionModel;
    const aesKey = sessionModel?.AesKey;
    const appId = sessionModel?.AppId;
    const token = sessionModel?.Token;

    if (token == null) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "未获取到正确的Token"
      };
    }

    //数据按照顺序组成数组
    const arrayList = [
      aesKey,
      token,
      appId,
      data?.RSASignPublicKey,
      data?.RSAEncryptPublicKey,
      data?.TimeStamp,
      data?.Nonce
    ];

    //获取会话的Hash数据
    const getRSAHashResult = await this.GetRSAHashAsync(arrayList);
    if (!getRSAHashResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: getRSAHashResult.Message
      };
    }

    //验证签名
    const rsaVerifyDataResult = await this.RSAVerifyDataAsync(
      getRSAHashResult.Data,
      data?.MsgSignature,
      data?.RSASignPublicKey
    );
    if (!rsaVerifyDataResult.IsSuccess) {
      return rsaVerifyDataResult;
    }


    //验证通过后解密AppId 用客户端自己的私钥进行解密
    const appIdDecryptResult = await this.RSADecryptAsync(appId, this.GetRSAEncryptKeyResult?.PrivateKey)
    if (!appIdDecryptResult.IsSuccess) {
      return appIdDecryptResult;
    }
    if (appIdDecryptResult.Data == null) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "未正确获取到AppId"
      };
    }

    //验证通过后对AesKey进行解密 用客户端自己的私钥进行解密
    const aesKeyDecryptResult = await this.RSADecryptAsync(aesKey, this.GetRSAEncryptKeyResult?.PrivateKey)
    if (!aesKeyDecryptResult.IsSuccess) {
      return aesKeyDecryptResult;
    }

    if (aesKeyDecryptResult.Data == null) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "未正确获取到AesKey"
      };
    }

    //最后将appid aeskey token 存储到sessionStorage里
    //这里也可以采取存储加密的AppId AesKey 和Token
    //sessionStorage.setItem('AppId', appIdDecryptResult.Data);
    //sessionStorage.setItem('AesKey', aesKeyDecryptResult.Data);
    //sessionStorage.setItem('Token', token);



    if (sessionModel != null) {
      sessionModel.AppId = appIdDecryptResult.Data;
      sessionModel.AesKey = aesKeyDecryptResult.Data;
      sessionModel.Token = token;
      sessionStorage.setItem('SessionModel', JSON.stringify(sessionModel))
    }

    return {
      IsSuccess: true,
      Data: null,
      Message: "success"
    };
  }


  // 使用async/await的版本
  //这里name 可用
  //签名用算法RSASSA-PKCS1-v1_5
  //加解密用RSA-OAEP
  //RSA-PSS 这个算法没用过 愿意测试自己搞一搞
  public async GenerateRSAKeysAsync(
    keyName: string,
    name: string,
    keykeyUsages: ReadonlyArray<KeyUsage>):
    Promise<GernerateKeyResult> {
    try {
      // 生成RSA密钥对
      const keyPair = await window.crypto.subtle.generateKey({
        name: name,
        modulusLength: 2048,
        publicExponent: new Uint8Array([0x01, 0x00, 0x01]), // 65537
        hash: "SHA-256",
      },
        true,
        keykeyUsages);

      // 导出公钥
      //这里spki意思就是Subject_Public_Key_Info 导出类型就是和C#里ImportSubjectPublicKeyInfo
      const publicKey = await window.crypto.subtle.exportKey("spki", keyPair.publicKey);

      // 导出私钥
      const privateKey = await window.crypto.subtle.exportKey("pkcs8", keyPair.privateKey);

      // 转换为Base64格式
      const hashArray1 = Array.from(new Uint8Array(publicKey));
      const rSAPublicKey = btoa(String.fromCharCode.apply(null, hashArray1));
      const hashArray2 = Array.from(new Uint8Array(privateKey));

      const rSAPrivateKey = btoa(String.fromCharCode.apply(null, hashArray2));

      // 存储到sessionStorage
      sessionStorage.setItem(`RSA${keyName}PublicKey`, rSAPublicKey);
      sessionStorage.setItem(`RSA${keyName}PrivateKey`, rSAPrivateKey);
      return {
        IsSuccess: true,
        PublicKey: rSAPublicKey,
        PrivateKey: rSAPrivateKey,
        Message: "Success",
      };
    } catch {
      return {
        IsSuccess: false,
        PublicKey: null,
        PrivateKey: null,
        Message: "生成密钥对时出错",
      };
    }
  }

  public async GetRSAHashAsync(arrayList: (string | undefined | null)[]):
    Promise<OperateResult<ArrayBuffer>> {
    try {
      // 对数组进行排序
      arrayList.sort();

      // 将数组元素拼接成字符串
      const raw = arrayList.join('');

      // 使用SubtleCrypto API计算SHA256哈希
      const encoder = new TextEncoder();
      const Data = encoder.encode(raw);

      return await crypto.subtle.digest('SHA-256', Data)
        .then(hash => {
          return {
            IsSuccess: true,
            Data: hash,
            Message: "Succss"
          };
        })
        .catch(() => {
          return {
            IsSuccess: false,
            Data: null,
            Message: '生成Hash值失败'
          };
        });
    } catch {
      return Promise.resolve({
        IsSuccess: false,
        Data: null,
        Message: '生成Hash值失败'
      });
    }
  }

  public async RSASignDataAsync(
    hash: ArrayBuffer | null,
    rsaSigningPrivateKey: string | null | undefined):
    Promise<{ IsSuccess: boolean, Data: string | null, Message: string }> {
    try {
      // 检查私钥格式
      if (!rsaSigningPrivateKey || typeof rsaSigningPrivateKey !== 'string') {
        return {
          IsSuccess: false,
          Data: null,
          Message: "私钥格式错误：私钥为空或格式不正确"
        };
      }

      // 检查私钥是否为有效的Base64字符串
      try {
        // 将Base64私钥转换为ArrayBuffer
        const PrivateKeyBinary = Uint8Array.from(atob(rsaSigningPrivateKey), c => c.charCodeAt(0));

        // 导入私钥 - 使用RSA-PKCS1-v1_5算法进行签名
        const PublicKey = await window.crypto.subtle.importKey(
          "pkcs8",
          PrivateKeyBinary,
          {
            name: "RSASSA-PKCS1-v1_5",
            hash: "SHA-256"
          },
          false,
          ["sign"]
        );

        if (hash == null) {
          return {
            IsSuccess: false,
            Data: null,
            Message: "Hash不能为null"
          };
        }

        // 使用私钥签名
        const signature = await window.crypto.subtle.sign(
          "RSASSA-PKCS1-v1_5",
          PublicKey,
          hash
        );

        // 将签名结果转换为Base64字符串
        const hashArray1 = Array.from(new Uint8Array(signature));
        const rsaSignData = btoa(String.fromCharCode.apply(null, hashArray1));

        return {
          IsSuccess: true,
          Data: rsaSignData,
          Message: "签名成功"
        };
      } catch {
        return {
          IsSuccess: false,
          Data: null,
          Message: "私钥Base64解码失败"
        };
      }
    } catch {
      return {
        IsSuccess: false,
        Data: null,
        Message: "RSA签名失败"
      };
    }
  }


  //这里验证签名是用服务端的签名公钥
  public async RSAVerifyDataAsync(
    hash: ArrayBuffer | null,
    signature: string | null | undefined,
    rsaSigningPrivateKey: string | null | undefined,
  ): Promise<{ IsSuccess: boolean, Data: string | null, Message: string }> {
    try {
      // 检查参数
      if (!rsaSigningPrivateKey || typeof rsaSigningPrivateKey !== 'string') {
        return {
          IsSuccess: false,
          Data: null,
          Message: "公钥格式错误：公钥为空或格式不正确"
        };
      }

      try {

        // 将Base64私钥转换为ArrayBuffer
        const PrivateKeyBinary = Uint8Array.from(atob(rsaSigningPrivateKey), c => c.charCodeAt(0));

        // 导入公钥 - 使用RSASSA-PKCS1-v1_5算法进行验证
        const PrivateKey = await window.crypto.subtle.importKey(
          "spki",
          PrivateKeyBinary,
          {
            name: "RSASSA-PKCS1-v1_5",
            hash: "SHA-256"
          },
          false,
          ["verify"]
        );

        if (signature == null) {
          return {
            IsSuccess: false,
            Data: null,
            Message: "签名不能为null"
          };
        }

        const signatureBinary = Uint8Array.from(atob(signature), c => c.charCodeAt(0));

        // 使用公钥验证签名
        if (hash == null) {
          return {
            IsSuccess: false,
            Data: null,
            Message: "hash值不能为null"
          };
        }
        const isValid = await window.crypto.subtle.verify(
          "RSASSA-PKCS1-v1_5",
          PrivateKey,
          signatureBinary,
          hash
        );

        if (!isValid) {
          return {
            IsSuccess: false,
            Data: null,
            Message: "签名验证失败"
          };
        }
        return {
          IsSuccess: true,
          Data: null,
          Message: "签名验证成功"
        };
      } catch {
        return {
          IsSuccess: false,
          Data: null,
          Message: "Base64解码失败"
        };
      }
    } catch {
      return {
        IsSuccess: false,
        Data: null,
        Message: "签名验证失败"
      };
    }
  }



  public async RSAEncryptAsync(
    encryptContent: string,
    rsaEncryptionPublicKey: string
  ): Promise<{ IsSuccess: boolean, Data: string | null, Message: string }> {
    try {
      // 检查参数
      if (!encryptContent || typeof encryptContent !== 'string') {
        return {
          IsSuccess: false,
          Data: null,
          Message: "加密的数据不能为空"
        };
      }

      if (!rsaEncryptionPublicKey || typeof rsaEncryptionPublicKey !== 'string') {
        return {
          IsSuccess: false,
          Data: null,
          Message: "公钥格式错误：公钥为空或格式不正确"
        };
      }

      try {
        // 将Base64公钥转换为ArrayBuffer
        const PublicKeyBinary = Uint8Array.from(atob(rsaEncryptionPublicKey), c => c.charCodeAt(0));

        // 导入公钥 - 使用RSA-OAEP算法进行验证
        const PublicKey = await window.crypto.subtle.importKey(
          "spki",
          PublicKeyBinary,
          {
            name: "RSA-OAEP",
            hash: "SHA-256"
          },
          false,
          ["encrypt"]
        );

        // 将字符串转换为UTF-8编码的字节数组
        const encoder = new TextEncoder();
        const DataToEncrypt = encoder.encode(encryptContent);

        // 使用公钥加密
        const encrypted = await window.crypto.subtle.encrypt(
          "RSA-OAEP",
          PublicKey,
          DataToEncrypt
        );

        // 将加密结果转换为Base64字符串
        const hashArray1 = Array.from(new Uint8Array(encrypted));
        const base64Encrypted = btoa(String.fromCharCode.apply(null, hashArray1));

        return {
          IsSuccess: true,
          Data: base64Encrypted,
          Message: "加密成功"
        };
      } catch {
        console.error('公钥导入错误');
        return {
          IsSuccess: false,
          Data: null,
          Message: "密钥导入失败"
        };
      }
    } catch {
      console.error('RSA加密错误');
      return {
        IsSuccess: false,
        Data: null,
        Message: "RSA加密失败"
      };
    }
  }


  public async RSADecryptAsync(
    encryptContent: string | null | undefined,
    rsaEncryptionPrivateKey: string | null | undefined,
  ): Promise<OperateResult<string>> {
    try {
      // 检查参数
      if (!encryptContent || typeof encryptContent !== 'string') {
        return {
          IsSuccess: false,
          Data: null,
          Message: "加密数据格式错误：数据为空或格式不正确"
        };
      }

      if (!rsaEncryptionPrivateKey || rsaEncryptionPrivateKey.length === 0) {
        return {
          IsSuccess: false,
          Data: null,
          Message: "私钥格式错误：私钥为空或格式不正确"
        };
      }

      try {

        const PrivateKeyBinary = Uint8Array.from(atob(rsaEncryptionPrivateKey), c => c.charCodeAt(0));
        // 导入私钥 - 使用RSA-OAEP算法进行解密
        const PrivateKey = await window.crypto.subtle.importKey(
          "pkcs8",
          PrivateKeyBinary,
          {
            name: "RSA-OAEP",
            hash: "SHA-256"
          },
          false,
          ["decrypt"]
        );

        const encryptBinary = Uint8Array.from(atob(encryptContent), c => c.charCodeAt(0));

        // 使用私钥解密
        const decrypted = await window.crypto.subtle.decrypt(
          "RSA-OAEP",
          PrivateKey,
          encryptBinary
        );

        // 将解密结果转换为字符串
        const decoder = new TextDecoder();
        const decryptedText = decoder.decode(decrypted);

        return {
          IsSuccess: true,
          Data: decryptedText,
          Message: "解密成功"
        };
      } catch {
        return {
          IsSuccess: false,
          Data: null,
          Message: "Base64解密失败"
        };
      }
    } catch {
      return {
        IsSuccess: false,
        Data: null,
        Message: "RSA解密失败"
      };
    }
  }




  /**
   * 将数字转化成ASCII码对应的字符，用于对明文进行补码
   * @param a 需要转化的数字
   * @returns 转化得到的字符
   */
  private Chr(a: number): string {
    const target = a & 0xFF;
    return String.fromCharCode(target);
  }

  /**
   * KCS7编码器
   * @param textLength 内容长度
   * @returns 补位字节数组
   */
  private KCS7Encoder(textLength: number): Uint8Array {
    const blockSize = 32;
    // 计算需要填充的位数
    let amountToPad = blockSize - (textLength % blockSize);
    if (amountToPad === 0) {
      amountToPad = blockSize;
    }

    // 获得补位所用的字符
    const padChr = this.Chr(amountToPad);
    let tmp = "";
    for (let index = 0; index < amountToPad; index++) {
      tmp += padChr;
    }

    // 将字符串转换为UTF-8编码的字节数组
    const encoder = new TextEncoder();
    return encoder.encode(tmp);
  }

  /**
   * AES加密
   * @param input 加密内容
   * @param iv 设置要用于对称算法的初始化向量（IV）
   * @param key 设置用于对称算法的密钥
   * @returns 返回加密后的Base64字符串
   */
  private async AESEncrypt(input: Uint8Array, iv: Uint8Array, key: Uint8Array): Promise<string> {
    try {
      // 导入密钥
      const cryptoKey = await crypto.subtle.importKey(
        'raw',
        key,
        { name: 'AES-CBC', length: 256 },
        false,
        ['encrypt']
      );

      // 自己进行PKCS7补位，用系统自己带的不行
      const msg = new Uint8Array(input.length + 32 - input.length % 32);
      msg.set(input, 0);
      const pad = this.KCS7Encoder(input.length);
      msg.set(pad, input.length);

      // 加密数据
      const encryptedData = await crypto.subtle.encrypt(
        { name: 'AES-CBC', iv: iv },
        cryptoKey,
        msg
      );

      // 转换为Base64
      const encryptedArray = new Uint8Array(encryptedData);
      return btoa(String.fromCharCode.apply(null, Array.from(encryptedArray)));
    } catch (error) {
      throw new Error(`AES加密失败: ${error}`);
    }
  }

  /**
   * 创建随机数
   * @param codeLen 随机数长度
   * @returns 随机字符串
   */
  public CreateRandCode(codeLen: number): string {
    const codeSerial = "2,3,4,5,6,7,a,c,d,e,f,h,i,j,k,m,n,p,r,s,t,A,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,U,V,W,X,Y,Z";
    if (codeLen === 0) {
      codeLen = 16;
    }
    const arr = codeSerial.split(',');
    let code = "";
    let randValue = -1;

    // 使用 JavaScript 内置的 Math.random() 生成随机数
    for (let i = 0; i < codeLen; i++) {
      randValue = Math.floor(Math.random() * (arr.length - 1));
      code += arr[randValue];
    }
    return code;
  }

  /**
   * AES对称加密
   * @param input 加密字符串
   * @param encodingAESKey AES对称密钥
   * @param appid 应用主键
   * @returns 返回加密后的Base64字符串
   */
  public async AESEncryptWithAppId(input: string, encodingAESKey: string, appid: string): Promise<string> {
    try {
      // 解码Base64密钥并添加等号
      const keyBase64 = encodingAESKey + "=";
      const keyBytes = Uint8Array.from(atob(keyBase64), c => c.charCodeAt(0));

      // 创建16字节的IV（从密钥的前16字节复制）
      const ivBytes = new Uint8Array(16);
      ivBytes.set(keyBytes.slice(0, 16));

      // 生成16字节的随机码
      const randCode = this.CreateRandCode(16);
      const bRand = new TextEncoder().encode(randCode);

      // 编码应用ID和输入字符串
      const bAppid = new TextEncoder().encode(appid);
      const btmpMsg = new TextEncoder().encode(input);

      // 获取消息长度并转换为网络字节序（大端序）
      const msgLength = btmpMsg.length;
      const bMsgLen = new Uint8Array(4);
      bMsgLen[0] = (msgLength >> 24) & 0xFF;
      bMsgLen[1] = (msgLength >> 16) & 0xFF;
      bMsgLen[2] = (msgLength >> 8) & 0xFF;
      bMsgLen[3] = msgLength & 0xFF;

      // 创建完整的消息字节数组
      const bMsg = new Uint8Array(bRand.length + bMsgLen.length + bAppid.length + btmpMsg.length);

      // 按顺序复制各部分数据
      bMsg.set(bRand, 0);
      bMsg.set(bMsgLen, bRand.length);
      bMsg.set(btmpMsg, bRand.length + bMsgLen.length);
      bMsg.set(bAppid, bRand.length + bMsgLen.length + btmpMsg.length);

      // 使用AES-CBC模式加密
      return await this.AESEncrypt(bMsg, ivBytes, keyBytes);
    } catch (error) {
      throw new Error(`AES加密失败: ${error}`);
    }
  }

  /**
   * 生成签名
   * @param token token值
   * @param timeStamp 时间戳
   * @param nonce 随机数
   * @param msgEncrypt 消息加密
   * @returns 返回签名结果
   */
  private async GenarateSinature(token: string, timeStamp: string, nonce: string, msgEncrypt: string): Promise<OperateResult<string>> {
    try {
      // 创建数组并排序
      const arrayList = [token, timeStamp, nonce, msgEncrypt];
      arrayList.sort();

      // 拼接字符串
      const raw = arrayList.join('');

      // 使用 SHA-1 计算哈希值
      const encoder = new TextEncoder();
      const dataToHash = encoder.encode(raw);
      const hashBuffer = await crypto.subtle.digest('SHA-1', dataToHash);

      // 将哈希值转换为十六进制字符串
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hash = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

      return {
        IsSuccess: true,
        Data: hash,
        Message: "Success"
      };
    } catch {
      return {
        IsSuccess: false,
        Data: null,
        Message: "生成签名失败"
      };
    }
  }

  /**
   * AES对称加密
   * @param encryptModelShould 待加密数据
   * @param sessionModel 会话实体
   * @returns 返回加密结果
   */
  public async AESEncryptGeneric<T>(encryptModelShould: T, sessionModel: SessionModel): Promise<OperateResult<AESEncryptModel>> {
    if (encryptModelShould == null) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "加密实体不能为空"
      };
    }

    const replyMsg = JSON.stringify(encryptModelShould);
    const timeStamp = new Date().toISOString();
    const nonce = this.CreateRandCode(10);

    let raw = "";
    try {
      raw = await this.AESEncryptWithAppId(replyMsg, sessionModel.AesKey, sessionModel.AppId);
    } catch {
      // 在严格模式下，如果不使用错误参数，可以直接省略参数
      return {
        IsSuccess: false,
        Data: null,
        Message: "数据加密错误"
      };
    }

    // 生成签名
    const genarateSinatureResult = await this.GenarateSinature(sessionModel.Token, timeStamp, nonce, raw);
    if (!genarateSinatureResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: genarateSinatureResult.Message
      };
    }
    const MsgSigature = genarateSinatureResult.Data || "";

    const aesEncryptModel = new AESEncryptModel();
    aesEncryptModel.Encrypt = raw;
    aesEncryptModel.MsgSignature = MsgSigature;
    aesEncryptModel.TimeStamp = timeStamp;
    aesEncryptModel.Nonce = nonce;

    return {
      IsSuccess: true,
      Data: aesEncryptModel,
      Message: "Success"
    };
  }

  /**
   * 获取会话数据
   * @returns 返回会话模型结果
   */
  public static GetSessionModelFromStorage(): OperateResult<SessionModel> {
    // 从 sessionStorage 获取会话模型
    const sessionModelJson = sessionStorage.getItem('SessionModel');
    let sessionModel: SessionModel | null = null;

    if (sessionModelJson) {
      try {
        const parsedModel = JSON.parse(sessionModelJson);
        sessionModel = new SessionModel(
          parsedModel.AesKey || "",
          parsedModel.AppId || "",
          parsedModel.Token || ""
        );
      } catch {
        // 解析失败时 sessionModel 保持为 null
      }
    }

    if (sessionModel == null) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "你暂时无法使用该软件！"
      };
    }

    if (!sessionModel.AesKey || sessionModel.AesKey.trim() === "" || sessionModel.AesKey.length !== 43) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "密钥不合法"
      };
    }

    // 从 sessionStorage 获取 AppId
    if (!sessionModel.AppId || sessionModel.AppId.trim() === "") {
      return {
        IsSuccess: false,
        Data: null,
        Message: "没有获取到正确AppID"
      };
    }

    // 从 sessionStorage 获取 Token
    if (!sessionModel.Token || sessionModel.Token.trim() === "") {
      return {
        IsSuccess: false,
        Data: null,
        Message: "没有获取到正确Token"
      };
    }

    return {
      IsSuccess: true,
      Data: sessionModel,
      Message: "Success"
    };
  }

  /**
   * AES对称加密
   * @param encryptModelShould 待加密数据
   * @returns 返回加密结果
   */
  public async AESEncryptSimple<T>(encryptModelShould: T): Promise<OperateResult<AESEncryptModel>> {
    // 获取 aesKey, appId, token
    const getSessionModelResult = Cryptography.GetSessionModelFromStorage();
    if (!getSessionModelResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: getSessionModelResult.Message
      };
    }

    // 确保 sessionModel 不为 null
    if (!getSessionModelResult.Data) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "会话模型数据为空"
      };
    }

    const sessionModel = getSessionModelResult.Data;

    // 返回AES对称加密数据
    return await this.AESEncryptGeneric(encryptModelShould, sessionModel);
  }

  /**
   * 验证签名
   * @param token token值
   * @param timeStamp 时间戳
   * @param nonce 随机数
   * @param msgEncrypt 加密消息
   * @param sigture 签名
   * @returns 返回验证结果
   */
  public async VerifySignature(token: string, timeStamp: string, nonce: string, msgEncrypt: string, sigture: string): Promise<OperateResult<null>> {
    // 生成签名
    const genarateSinatureResult = await this.GenarateSinature(token, timeStamp, nonce, msgEncrypt);
    if (!genarateSinatureResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: genarateSinatureResult.Message
      };
    }
    const hash = genarateSinatureResult.Data || "";

    // 比较签名是否正确
    if (hash !== sigture) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "签名验证失败"
      };
    }

    return {
      IsSuccess: true,
      Data: null,
      Message: "Success"
    };
  }

  /**
   * 解码2
   * @param decrypted 解密内容
   * @returns 解码后的字节数组
   */
  private Decode2(decrypted: Uint8Array): Uint8Array {
    const pad = decrypted[decrypted.length - 1];
    let padValue = pad;

    if (padValue < 1 || padValue > 32) {
      padValue = 0;
    }

    const res = new Uint8Array(decrypted.length - padValue);
    res.set(decrypted.slice(0, decrypted.length - padValue));

    return res;
  }

  /**
   * AES解密
   * @param input 加密内容
   * @param iv 设置要用于对称算法的初始化向量（IV）
   * @param key 设置用于对称算法的密钥
   * @returns 解密后的字节数组
   */
  private async AESDecrypt(input: string, iv: Uint8Array, key: Uint8Array): Promise<Uint8Array> {
    try {
      // 导入密钥
      const cryptoKey = await crypto.subtle.importKey(
        'raw',
        key,
        { name: 'AES-CBC', length: 256 },
        false,
        ['decrypt']
      );

      // 将Base64字符串转换为字节数组
      const xXml = Uint8Array.from(atob(input), c => c.charCodeAt(0));

      // 创建足够大的缓冲区
      const msg = new Uint8Array(xXml.length + 32 - xXml.length % 32);
      msg.set(xXml, 0);

      // 解密数据
      const decryptedData = await crypto.subtle.decrypt(
        { name: 'AES-CBC', iv: iv },
        cryptoKey,
        xXml
      );

      // 使用 Decode2 方法处理解密后的数据
      return this.Decode2(new Uint8Array(decryptedData));
    } catch (error) {
      throw new Error(`AES解密失败: ${error}`);
    }
  }

  /**
   * 解密方法
   * @param input 密文
   * @param encodingAESKey AES对称密钥
   * @param appid 应用ID（输出参数）
   * @returns 解密后的明文
   */
  public async AESDecryptWithAppId(input: string, encodingAESKey: string, appid: { value: string }): Promise<string> {
    try {
      // 解码Base64密钥并添加等号
      const keyBase64 = encodingAESKey + "=";
      const keyBytes = Uint8Array.from(atob(keyBase64), c => c.charCodeAt(0));

      // 创建16字节的IV（从密钥的前16字节复制）
      const ivBytes = new Uint8Array(16);
      ivBytes.set(keyBytes.slice(0, 16));

      // 解密数据
      const btmpMsg = await this.AESDecrypt(input, ivBytes, keyBytes);

      // 从第16个字节开始读取4个字节作为消息长度（网络字节序）
      const lenBytes = btmpMsg.slice(16, 20);
      const len = (lenBytes[0] << 24) | (lenBytes[1] << 16) | (lenBytes[2] << 8) | lenBytes[3];

      // 从第20个字节开始读取消息内容
      const bMsg = btmpMsg.slice(20, 20 + len);

      // 从消息内容后面读取应用ID
      const bAppid = btmpMsg.slice(20 + len);

      // 将字节数组转换为字符串
      const oriMsg = new TextDecoder().decode(bMsg);
      appid.value = new TextDecoder().decode(bAppid);

      return oriMsg;
    } catch (error) {
      throw new Error(`AES解密失败: ${error}`);
    }
  }

  /**
   * AES对称数据解密
   * @param aesEncryptModel AES加密数据
   * @param sessionModel 会话实体类
   * @returns 返回解密后的实体
   */
  public async AESDecryptGeneric<TResult>(aesEncryptModel: AESEncryptModel, sessionModel: SessionModel): Promise<OperateResult<TResult>> {
    // 验证签名
    const verifySignatureResult = await this.VerifySignature(
      sessionModel.Token,
      aesEncryptModel.TimeStamp,
      aesEncryptModel.Nonce,
      aesEncryptModel.Encrypt,
      aesEncryptModel.MsgSignature
    );

    if (!verifySignatureResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: verifySignatureResult.Message
      };
    }

    // 解密
    const appid = { value: "" };
    let sMsg: string;

    try {
      sMsg = await this.AESDecryptWithAppId(aesEncryptModel.Encrypt, sessionModel.AesKey, appid);
    } catch (error) {
      if (error instanceof Error && error.message.includes("Base64")) {
        return {
          IsSuccess: false,
          Data: null,
          Message: "解码Base64错误"
        };
      } else {
        return {
          IsSuccess: false,
          Data: null,
          Message: "数据解密出错"
        };
      }
    }

    if (appid.value !== sessionModel.AppId) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "验证AppID失败"
      };
    }

    try {
      // 将JSON字符串转换为对象
      const result = JSON.parse(sMsg) as TResult;
      return {
        IsSuccess: true,
        Data: result,
        Message: "Success"
      };
    } catch {
      return {
        IsSuccess: false,
        Data: null,
        Message: "解析JSON数据失败"
      };
    }
  }

  /**
   * AES对称解密
   * @param aesEncryptModel AES加密数据
   * @returns 返回解密后的实体
   */
  public async AESDecryptSimple<TResult>(aesEncryptModel: AESEncryptModel): Promise<OperateResult<TResult>> {
    // 获取 aesKey, appId, token
    const getSessionModelResult = this.GetSessionModelFromStorage();
    if (!getSessionModelResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: getSessionModelResult.Message
      };
    }

    // 确保 sessionModel 不为 null
    if (!getSessionModelResult.Data) {
      return {
        IsSuccess: false,
        Data: null,
        Message: "会话模型数据为空"
      };
    }

    const sessionModel = getSessionModelResult.Data;

    // 返回AES对称解密数据
    return await this.AESDecryptGeneric<TResult>(aesEncryptModel, sessionModel);
  }

}







