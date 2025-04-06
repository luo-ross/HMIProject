import {
  CommonUtils,
  SessionRequestModel,
  type Data,
  type GernerateKeyResult,
  type OperateResult
} from '../scripts/utils'



export class cryptography {

  public getRSASignKeyResult: GernerateKeyResult | null = null;
  public getRSAEncryptKeyResult: GernerateKeyResult | null = null;
  public aesKeyDecryptResult: string | null = null;
  public appIdDecryptResult: string | null = null;
  private static instance: cryptography;
  private cryptography: cryptography | null = null;

  private constructor() { }

  public static getInstance(): cryptography {
    if (!cryptography.instance) {
      cryptography.instance = new cryptography();
    }
    return cryptography.instance;
  }

  // 获取用户名
  public async initDefaultKeys(): Promise<void> {

    this.getRSASignKeyResult = await this.getRSAKeyAsync("Sign");
    //如果缓存里没有，则从新创建
    if (!this.getRSASignKeyResult.IsSuccess) {
      //创建签名密钥
      this.getRSASignKeyResult = await this.generateRSAKeysAsync("Sign", "RSASSA-PKCS1-v1_5", ["sign", "verify"]);
      if (!this.getRSASignKeyResult.IsSuccess) {
        CommonUtils.showErrorMsg(this.getRSASignKeyResult.Message);
        return;
      }
    }

    this.getRSAEncryptKeyResult = await this.getRSAKeyAsync("Encrypt");
    if (!this.getRSAEncryptKeyResult.IsSuccess) {
      //创建加密密钥
      this.getRSAEncryptKeyResult = await this.generateRSAKeysAsync("Encrypt", "RSA-OAEP", ["encrypt", "decrypt"]);
      if (!this.getRSAEncryptKeyResult.IsSuccess) {
        CommonUtils.showErrorMsg(this.getRSAEncryptKeyResult.Message);
        return;
      }
    }

    // 将Date对象转换为Unix时间戳（毫秒）
    const timestamp = new Date().getTime();
    //创建会话请求
    const sessionRequestModel = new SessionRequestModel();

    sessionRequestModel.RSAEncryptPublicKey = this.getRSASignKeyResult.PublicKey;
    sessionRequestModel.RSASignPublicKey = this.getRSASignKeyResult.PublicKey;
    sessionRequestModel.RSAEncryptPublicKey = this.getRSAEncryptKeyResult.PublicKey;
    sessionRequestModel.Nonce = CommonUtils.createRandCode(10).toString();
    sessionRequestModel.TimeStamp = timestamp.toString();
    sessionRequestModel.AudienceType = "WebAudience";

    const arrayList = [
      sessionRequestModel.RSASignPublicKey,
      sessionRequestModel.RSAEncryptPublicKey,
      sessionRequestModel.Nonce,
      sessionRequestModel.TimeStamp,
    ];

    //获取会话的Hash数据
    const getRSAHashResult = await this.getRSAHashAsync(arrayList);
    if (!getRSAHashResult.IsSuccess) {
      CommonUtils.showErrorMsg(getRSAHashResult.Message);
      return;
    }

    const rsaSignDataResult = await this.rsaSignDataAsync(getRSAHashResult.Data, this.getRSASignKeyResult?.PrivateKey);
    if (!rsaSignDataResult.IsSuccess) {
      CommonUtils.showErrorMsg(rsaSignDataResult.Message);
      return;
    }
    sessionRequestModel.MsgSignature = rsaSignDataResult.Data;

    const result = await CommonUtils.ajaxPost("/api/v1/General/GetSessionModel", sessionRequestModel);
    const getSessionModelResult = await this.getSessionModel(result);

    if (!getSessionModelResult.IsSuccess) {
      CommonUtils.showErrorMsg(getSessionModelResult.Message);
      return;
    }

    return;
  }

  public async getRSAKeyAsync(keyName: string): Promise<GernerateKeyResult> {

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

  public async getSessionModel(operateResult: OperateResult<Data>)
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
    const getRSAHashResult = await this.getRSAHashAsync(arrayList);
    if (!getRSAHashResult.IsSuccess) {
      return {
        IsSuccess: false,
        Data: null,
        Message: getRSAHashResult.Message
      };
    }

    //验证签名
    const rsaVerifyDataResult = await this.rsaVerifyDataAsync(
      getRSAHashResult.Data,
      data?.MsgSignature,
      data?.RSASignPublicKey
    );
    if (!rsaVerifyDataResult.IsSuccess) {
      return rsaVerifyDataResult;
    }


    //验证通过后对AesKey进行解密 用客户端自己的私钥进行解密
    const aesKeyDecryptResult = await this.rsaDecryptAsync(aesKey, this.getRSAEncryptKeyResult?.PrivateKey)
    if (!aesKeyDecryptResult.IsSuccess) {
      return aesKeyDecryptResult;
    }


    //验证通过后解密AppId 用客户端自己的私钥进行解密
    const appIdDecryptResult = await this.rsaDecryptAsync(appId, this.getRSAEncryptKeyResult?.PrivateKey)
    if (!appIdDecryptResult.IsSuccess) {
      return appIdDecryptResult;
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
  public async generateRSAKeysAsync(
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
        Message: null,
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

  public async getRSAHashAsync(arrayList: (string | undefined | null)[]):
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

  public async rsaSignDataAsync(
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
  public async rsaVerifyDataAsync(
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



  public async rsaEncryptAsync(
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


  public async rsaDecryptAsync(
    encryptContent: string | null | undefined,
    rsaEncryptionPrivateKey: string | null | undefined,
  ): Promise<{ IsSuccess: boolean, Data: string | null, Message: string | null }> {
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
}











