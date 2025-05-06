import { AESEncryptModel } from "../../Models/WebAPI/AESEncryptModel";
import { MemoryCacheKey } from "../../Models/WebAPI/MemoryCacheKey";
import { SessionModel } from "../../Models/WebAPI/SessionModel";
import { SessionResultModel } from "../../Models/WebAPI/SessionResultModel";
import { RSAType } from "../Enums/RSAType";
import { GenericOperateResult, SimpleOperateResult } from "../OperateResult/OperateResult";
import { Utils } from "../Utils";

export class Cryptography {
  private static Instance: Cryptography;
  public Utils: Utils;
  private constructor() {
    this.Utils = new Utils();
  }
  public static GetInstance(): Cryptography {
    if (!Cryptography.Instance) {
      Cryptography.Instance = new Cryptography();
    }
    return Cryptography.Instance;
  }

  /// <summary>
  /// 初始化RSA非对称秘钥数据
  /// </summary>
  public async InitRSASecurityKeyDataAsync(): Promise<SimpleOperateResult> {

    const rsaSignPublicKey = sessionStorage.getItem(MemoryCacheKey.GlobalRSASignPublicKey);
    const rsaSignPrivateKey = sessionStorage.getItem(MemoryCacheKey.GlobalRSASignPrivateKey);
    const rsaEncryptPublicKey = sessionStorage.getItem(MemoryCacheKey.GlobalRSAEncryptPublicKey);
    const rsaEncryptPrivateKey = sessionStorage.getItem(MemoryCacheKey.GlobalRSAEncryptPrivateKey);

    if (rsaSignPublicKey == null
      || rsaSignPrivateKey == null
      || rsaEncryptPublicKey == null
      || rsaEncryptPrivateKey == null) {
      let generateRSAKeysResult = await this.GenerateRSAKeysAsync(RSAType.Sign, "RSASSA-PKCS1-v1_5", ["sign", "verify"]);
      if (!generateRSAKeysResult.IsSuccess) {
        return generateRSAKeysResult;
      }
      generateRSAKeysResult = await this.GenerateRSAKeysAsync(RSAType.Encrypt, "RSA-OAEP", ["encrypt", "decrypt"]);
      if (!generateRSAKeysResult.IsSuccess) {
        return generateRSAKeysResult;
      }
    }

    return SimpleOperateResult.CreateSuccessResult();

  }


  /**
   * 获取SHA256哈希值
   * @param hashContent 哈希内容
   * @returns 返回哈希值的十六进制字符串
   */
  public async GetSHA256HashCode(hashContent: string | null): Promise<GenericOperateResult<string>> {
    try {
      if (hashContent == null) {
        return GenericOperateResult.CreateFailResult('哈希内容不能为空');
      }
      // 将字符串转换为 UTF-8 编码的字节数组
      const encoder = new TextEncoder();
      const data = encoder.encode(hashContent);

      // 计算 SHA-256 哈希值
      const hashBuffer = await crypto.subtle.digest('SHA-256', data);

      // 将哈希值转换为十六进制字符串
      const hashArray = Array.from(new Uint8Array(hashBuffer));
      const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

      return GenericOperateResult.CreateSuccessResult(hashHex.toUpperCase());
    } catch (error) {
      return GenericOperateResult.CreateFailResult(`计算SHA256哈希值失败: ${error}`);
    }
  }






  public async GetSessionModel(operateResult: GenericOperateResult<SessionResultModel>)
    : Promise<SimpleOperateResult> {
    if (!operateResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(operateResult);
    }

    const data = operateResult.Data;
    const sessionModel = data?.SessionModel;
    const aesKey = sessionModel?.AesKey;
    const appId = sessionModel?.AppId;
    const token = sessionModel?.Token;

    if (token == null) {
      return SimpleOperateResult.CreateFailResult("未获取到正确的Token");
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
      return GenericOperateResult.CreateFailResult(getRSAHashResult);
    }

    //验证签名
    const rsaVerifyDataResult = await this.RSAVerifyDataAsync(
      getRSAHashResult.Data,
      data?.MsgSignature,
      data?.RSASignPublicKey
    );
    if (!rsaVerifyDataResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(rsaVerifyDataResult);
    }

    //验证通过后解密AppId 用客户端自己的私钥进行解密
    const appIdDecryptResult = await this.RSADecryptAsync(appId, sessionStorage.getItem(MemoryCacheKey.GlobalRSAEncryptPrivateKey))
    if (!appIdDecryptResult.IsSuccess) {
      return appIdDecryptResult;
    }
    if (appIdDecryptResult.Data == null) {

      return SimpleOperateResult.CreateFailResult("未正确获取到AppId");
    }

    //验证通过后对AesKey进行解密 用客户端自己的私钥进行解密
    const aesKeyDecryptResult = await this.RSADecryptAsync(aesKey, sessionStorage.getItem(MemoryCacheKey.GlobalRSAEncryptPrivateKey))
    if (!aesKeyDecryptResult.IsSuccess) {
      return aesKeyDecryptResult;
    }

    if (aesKeyDecryptResult.Data == null) {
      return SimpleOperateResult.CreateFailResult("未正确获取到AesKey");
    }

    if (sessionModel != null) {
      sessionModel.AppId = appIdDecryptResult.Data;
      sessionModel.AesKey = aesKeyDecryptResult.Data;
      sessionModel.Token = token;
      sessionStorage.setItem('SessionModel', JSON.stringify(sessionModel))
    }



    return SimpleOperateResult.CreateSuccessResult();
  }


  // 使用async/await的版本
  //这里name 可用
  //签名用算法RSASSA-PKCS1-v1_5
  //加解密用RSA-OAEP
  //RSA-PSS 这个算法没用过 愿意测试自己搞一搞
  public async GenerateRSAKeysAsync(
    rsaType: RSAType,
    name: string,
    keykeyUsages: ReadonlyArray<KeyUsage>):
    Promise<SimpleOperateResult> {
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
      switch (rsaType) {
        case RSAType.Sign:
          sessionStorage.setItem(MemoryCacheKey.GlobalRSASignPublicKey, rSAPublicKey);
          sessionStorage.setItem(MemoryCacheKey.GlobalRSASignPrivateKey, rSAPrivateKey);
          break;
        case RSAType.Encrypt:
          sessionStorage.setItem(MemoryCacheKey.GlobalRSAEncryptPublicKey, rSAPublicKey);
          sessionStorage.setItem(MemoryCacheKey.GlobalRSAEncryptPrivateKey, rSAPrivateKey);
          break;
      }
      return SimpleOperateResult.CreateSuccessResult();
    } catch {
      return SimpleOperateResult.CreateFailResult("生成密钥出错");
    }
  }

  public async GetRSAHashAsync(arrayList: (string | undefined | null)[]):
    Promise<GenericOperateResult<ArrayBuffer>> {
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

          return GenericOperateResult.CreateSuccessResult(hash);
        })
        .catch(() => {
          return GenericOperateResult.CreateFailResult('生成Hash值失败');
        });
    } catch {
      return GenericOperateResult.CreateFailResult('生成Hash值失败');
    }
  }

  public async RSASignDataAsync(
    hash: ArrayBuffer | null,
    rsaSigningPrivateKey: string | null | undefined):
    Promise<GenericOperateResult<string>> {
    try {
      // 检查私钥格式
      if (!rsaSigningPrivateKey || typeof rsaSigningPrivateKey !== 'string') {
        return GenericOperateResult.CreateFailResult("私钥格式错误：私钥为空或格式不正确");
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
          return GenericOperateResult.CreateFailResult("Hash不能为null");
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
        return GenericOperateResult.CreateSuccessResult(rsaSignData);
      } catch {
        return GenericOperateResult.CreateFailResult("私钥Base64解码失败");
      }
    } catch {
      return GenericOperateResult.CreateFailResult("RSA签名失败");
    }
  }


  //这里验证签名是用服务端的签名公钥
  public async RSAVerifyDataAsync(
    hash: ArrayBuffer | null,
    signature: string | null | undefined,
    rsaSigningPrivateKey: string | null | undefined,
  ): Promise<SimpleOperateResult> {
    try {
      // 检查参数
      if (!rsaSigningPrivateKey || typeof rsaSigningPrivateKey !== 'string') {
        return SimpleOperateResult.CreateFailResult("公钥格式错误：公钥为空或格式不正确");
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
          return SimpleOperateResult.CreateFailResult("签名不能为null");
        }

        const signatureBinary = Uint8Array.from(atob(signature), c => c.charCodeAt(0));

        // 使用公钥验证签名
        if (hash == null) {

          return SimpleOperateResult.CreateFailResult("hash值不能为null");
        }
        const isValid = await window.crypto.subtle.verify(
          "RSASSA-PKCS1-v1_5",
          PrivateKey,
          signatureBinary,
          hash
        );

        if (!isValid) {
          return SimpleOperateResult.CreateFailResult("签名验证失败");
        }
        return SimpleOperateResult.CreateSuccessResult();
      } catch {
        return SimpleOperateResult.CreateFailResult("Base64解码失败");
      }
    } catch {
      return SimpleOperateResult.CreateFailResult("签名验证失败");
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
  ): Promise<GenericOperateResult<string>> {
    try {
      // 检查参数
      if (!encryptContent || typeof encryptContent !== 'string') {
        return GenericOperateResult.CreateFailResult("加密数据格式错误：数据为空或格式不正确");
      }

      if (!rsaEncryptionPrivateKey || rsaEncryptionPrivateKey.length === 0) {
        return GenericOperateResult.CreateFailResult("私钥格式错误：私钥为空或格式不正确");
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

        return GenericOperateResult.CreateSuccessResult(decryptedText);
      } catch {
        return GenericOperateResult.CreateFailResult("Base64解密失败");
      }
    } catch {
      return GenericOperateResult.CreateFailResult("RSA解密失败");
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
  public async AESEncryptWithAppId(input: string, encodingAESKey: string | null, appid: string | null): Promise<string> {
    try {

      if (appid == null) {
        throw new Error("appid不能为空");
      }

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
      const networkOrderLength = this.HostToNetworkOrder(msgLength);

      // 将网络字节序的长度转换为字节数组
      const bMsgLen = new Uint8Array(4);
      bMsgLen[0] = networkOrderLength & 0xFF; // 最低有效字节
      bMsgLen[1] = (networkOrderLength >> 8) & 0xFF;
      bMsgLen[2] = (networkOrderLength >> 16) & 0xFF;
      bMsgLen[3] = (networkOrderLength >> 24) & 0xFF; // 最高有效字节

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
  private async GenarateSinature(token: string | null, timeStamp: string | null, nonce: string | null, msgEncrypt: string | null): Promise<GenericOperateResult<string>> {
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
      return GenericOperateResult.CreateSuccessResult(hash);
    } catch {
      return GenericOperateResult.CreateFailResult("生成签名失败");
    }
  }

  /**
   * AES对称加密
   * @param encryptModelShould 待加密数据
   * @param sessionModel 会话实体
   * @returns 返回加密结果
   */
  public async AESEncryptGeneric<T>(encryptModelShould: T, sessionModel: SessionModel): Promise<GenericOperateResult<AESEncryptModel>> {
    if (encryptModelShould == null) {
      return GenericOperateResult.CreateFailResult("加密实体不能为空");
    }

    const replyMsg = JSON.stringify(encryptModelShould);
    const timeStamp = new Date().toISOString();
    const nonce = this.CreateRandCode(10);

    let raw = "";
    try {
      raw = await this.AESEncryptWithAppId(replyMsg, sessionModel.AesKey, sessionModel.AppId);
    } catch {
      return GenericOperateResult.CreateFailResult("数据加密错误");
    }

    // 生成签名
    const genarateSinatureResult = await this.GenarateSinature(sessionModel.Token, timeStamp, nonce, raw);
    if (!genarateSinatureResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(genarateSinatureResult);
    }
    const MsgSigature = genarateSinatureResult.Data || "";

    const aesEncryptModel = new AESEncryptModel();
    aesEncryptModel.Encrypt = raw;
    aesEncryptModel.MsgSignature = MsgSigature;
    aesEncryptModel.TimeStamp = timeStamp;
    aesEncryptModel.Nonce = nonce;


    return GenericOperateResult.CreateSuccessResult(aesEncryptModel);
  }

  /**
   * 获取会话数据
   * @returns 返回会话模型结果
   */
  public GetSessionModelFromStorage(): GenericOperateResult<SessionModel> {
    // 从 sessionStorage 获取会话模型
    const sessionModelJson = sessionStorage.getItem('SessionModel');
    let sessionModel: SessionModel | null = null;

    if (sessionModelJson) {
      try {
        const parsedModel = JSON.parse(sessionModelJson);
        sessionModel = new SessionModel();
        sessionModel.AesKey = parsedModel.AesKey;
        sessionModel.AppId = parsedModel.AppId;
        sessionModel.Token = parsedModel.Token;
      } catch {
        // 解析失败时 sessionModel 保持为 null
      }
    }

    if (sessionModel == null) {
      return GenericOperateResult.CreateFailResult("请先创建会话！");
    }

    if (!sessionModel.AesKey || sessionModel.AesKey.trim() === "" || sessionModel.AesKey.length !== 43) {
      return GenericOperateResult.CreateFailResult("AesKey错误！");
    }

    if (!sessionModel.AppId || sessionModel.AppId.trim() === "") {
      return GenericOperateResult.CreateFailResult("AppId错误！");
    }

    if (!sessionModel.Token || sessionModel.Token.trim() === "") {
      return GenericOperateResult.CreateFailResult("Token错误！");
    }
    return GenericOperateResult.CreateSuccessResult(sessionModel);
  }

  /**
   * AES对称加密
   * @param encryptModelShould 待加密数据
   * @returns 返回加密结果
   */
  public async AESEncryptSimple<T>(encryptModelShould: T): Promise<GenericOperateResult<AESEncryptModel>> {
    // 获取 aesKey, appId, token
    const getSessionModelResult = this.GetSessionModelFromStorage();
    if (!getSessionModelResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(getSessionModelResult);
    }

    // 确保 sessionModel 不为 null
    if (!getSessionModelResult.Data) {
      return GenericOperateResult.CreateFailResult("会话模型数据为空");
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
  public async VerifySignature(token: string | null, timeStamp: string | null, nonce: string | null, msgEncrypt: string | null, sigture: string | null): Promise<SimpleOperateResult> {
    // 生成签名
    const genarateSinatureResult = await this.GenarateSinature(token, timeStamp, nonce, msgEncrypt);
    if (!genarateSinatureResult.IsSuccess) {

      return genarateSinatureResult;

    }
    const hash = genarateSinatureResult.Data || "";

    // 比较签名是否正确
    if (hash !== sigture) {
      return SimpleOperateResult.CreateFailResult("签名验证失败");
    }

    return SimpleOperateResult.CreateSuccessResult();
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
  private async AESDecrypt(input: string | null, iv: Uint8Array, key: Uint8Array): Promise<Uint8Array> {
    try {

      if (input == null) {
        throw new Error('AES解密失败');
      }


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
  public async AESDecryptWithAppId(input: string | null, encodingAESKey: string | null, appid: { value: string }): Promise<string> {
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
  public async AESDecryptGeneric<TResult>(aesEncryptModel: AESEncryptModel | null, sessionModel: SessionModel, type: new () => TResult): Promise<GenericOperateResult<TResult>> {
    if (aesEncryptModel == null) {
      return GenericOperateResult.CreateFailResult('加密数据为null');
    }

    // 验证签名
    const verifySignatureResult = await this.VerifySignature(
      sessionModel.Token,
      aesEncryptModel.TimeStamp,
      aesEncryptModel.Nonce,
      aesEncryptModel.Encrypt,
      aesEncryptModel.MsgSignature
    );

    if (!verifySignatureResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(verifySignatureResult);
    }

    // 解密
    const appid = { value: "" };
    let sMsg: string;

    try {
      sMsg = await this.AESDecryptWithAppId(aesEncryptModel.Encrypt, sessionModel.AesKey, appid);
    } catch (error) {
      if (error instanceof Error && error.message.includes("Base64")) {
        return GenericOperateResult.CreateFailResult("解码Base64错误");
      } else {
        return GenericOperateResult.CreateFailResult("数据解密出错");
      }
    }

    if (appid.value !== sessionModel.AppId) {
      return GenericOperateResult.CreateFailResult("验证AppID失败");
    }

    try {
      // 将JSON字符串转换为对象
      const result = this.Utils.ToObject<TResult>(sMsg, type);
      return GenericOperateResult.CreateSuccessResult(result);
    } catch {
      return GenericOperateResult.CreateFailResult("解析JSON数据失败");
    }
  }

  /**
   * AES对称解密
   * @param aesEncryptModel AES加密数据
   * @returns 返回解密后的实体
   */
  public async AESDecryptSimple<TResult>(aesEncryptModel: AESEncryptModel | null, type: new () => TResult): Promise<GenericOperateResult<TResult>> {
    // 获取 aesKey, appId, token
    const getSessionModelResult = this.GetSessionModelFromStorage();
    if (!getSessionModelResult.IsSuccess) {
      return GenericOperateResult.CreateFailResult(getSessionModelResult.Message);
    }

    // 确保 sessionModel 不为 null
    if (!getSessionModelResult.Data) {
      return GenericOperateResult.CreateFailResult("会话模型数据为空");
    }

    const sessionModel = getSessionModelResult.Data;

    // 返回AES对称解密数据
    return await this.AESDecryptGeneric<TResult>(aesEncryptModel, sessionModel, type);
  }

  /**
   * 将短值由网络字节顺序转换为主机字节顺序。
   * @param inval 以网络字节顺序表示的要转换的数字。
   * @returns 转换后的数字
   */
  public HostToNetworkOrder(inval: number): number {
    let outval = 0;
    for (let i = 0; i < 4; i++) {
      outval = (outval << 8) + ((inval >> (i * 8)) & 255);
    }
    return outval;
  }




}







