var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
//可以在该js里直接调用
var getRSASignKeyResult;
var getRSAEncryptKeyResult;
var aesKeyDecryptResult;
var appIdDecryptResult;
function initDefaultKeys() {
    return __awaiter(this, void 0, void 0, function* () {
        getRSASignKeyResult = yield getRSAKeyAsync("Sign");
        //如果缓存里没有，则从新创建
        if (!getRSASignKeyResult.isSuccess) {
            //创建签名密钥
            getRSASignKeyResult = yield generateRSAKeysAsync("Sign", "RSASSA-PKCS1-v1_5", ["sign", "verify"]);
            if (!getRSASignKeyResult.isSuccess) {
                ShowErrorMsg(getRSASignKeyResult.message);
                return;
            }
        }
        getRSAEncryptKeyResult = yield getRSAKeyAsync("Encrypt");
        if (!getRSAEncryptKeyResult.isSuccess) {
            //创建加密密钥
            getRSAEncryptKeyResult = yield generateRSAKeysAsync("Encrypt", "RSA-OAEP", ["encrypt", "decrypt"]);
            if (!getRSAEncryptKeyResult.isSuccess) {
                ShowErrorMsg(getRSAEncryptKeyResult.message);
                return;
            }
        }
        // 将Date对象转换为Unix时间戳（毫秒）
        var timestamp = new Date().getTime();
        //创建会话请求
        var sessionRequestModel = {
            RSASignPublicKey: getRSASignKeyResult.publicKey,
            RSAEncryptPublicKey: getRSAEncryptKeyResult.publicKey,
            Nonce: createRandCode(10).toString(),
            TimeStamp: timestamp.toString(),
            MsgSignature: null,
            AudiencesType: "Web"
        };
        const arrayList = [
            sessionRequestModel.RSASignPublicKey,
            sessionRequestModel.RSAEncryptPublicKey,
            sessionRequestModel.Nonce,
            sessionRequestModel.TimeStamp,
        ];
        //获取会话的Hash数据
        var getRSAHashResult = yield getRSAHashAsync(arrayList);
        if (!getRSAHashResult.isSuccess) {
            ShowErrorMsg(getRSAHashResult.message);
            return;
        }
        var rsaSignDataResult = yield rsaSignDataAsync(getRSAHashResult.data, getRSASignKeyResult.privateKey);
        if (!rsaSignDataResult.isSuccess) {
            ShowErrorMsg(rsaSignDataResult.message);
            return;
        }
        sessionRequestModel.MsgSignature = rsaSignDataResult.data;
        const result = yield makeAjaxRequestAsync("/api/v1/General/GetSessionModel", sessionRequestModel);
        var getSessionModelResult = yield getSessionModel(result);
        if (!getSessionModelResult.isSuccess) {
            ShowErrorMsg(getSessionModelResult.message);
            return;
        }
    });
}
function getSessionModel(operateResult) {
    return __awaiter(this, void 0, void 0, function* () {
        if (!operateResult.IsSuccess) {
            return {
                isSuccess: true,
                data: null,
                message: operateResult.Message
            };
        }
        var data = operateResult.Data;
        var sessionModel = data.SessionModel;
        var aesKey = sessionModel.AesKey;
        var appId = sessionModel.AppId;
        var token = sessionModel.Token;
        //数据按照顺序组成数组
        var arrayList = [
            aesKey,
            token,
            appId,
            data.RSASignPublicKey,
            data.RSAEncryptPublicKey,
            data.TimeStamp,
            data.Nonce
        ];
        //获取会话的Hash数据
        var getRSAHashResult = yield getRSAHashAsync(arrayList);
        if (!getRSAHashResult.isSuccess) {
            return getRSAHashResult;
        }
        //验证签名
        var rsaVerifyDataResult = yield rsaVerifyDataAsync(getRSAHashResult.data, data.MsgSignature, data.RSASignPublicKey);
        if (!rsaVerifyDataResult.isSuccess) {
            return rsaVerifyDataResult;
        }
        //验证通过后对AesKey进行解密 用客户端自己的私钥进行解密
        var aesKeyDecryptResult = yield rsaDecryptAsync(aesKey, getRSAEncryptKeyResult.privateKey);
        if (!aesKeyDecryptResult.isSuccess) {
            return aesKeyDecryptResult;
        }
        //验证通过后解密AppId 用客户端自己的私钥进行解密
        var appIdDecryptResult = yield rsaDecryptAsync(appId, getRSAEncryptKeyResult.privateKey);
        if (!appIdDecryptResult.isSuccess) {
            return appIdDecryptResult;
        }
        return {
            isSuccess: true,
            data: null,
            message: "success"
        };
    });
}
// 使用async/await的版本
//这里name 可用
//签名用算法RSASSA-PKCS1-v1_5
//加解密用RSA-OAEP
//RSA-PSS 这个算法没用过 愿意测试自己搞一搞
function generateRSAKeysAsync(keyName, name, keykeyUsages) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 生成RSA密钥对
            const keyPair = yield window.crypto.subtle.generateKey({
                name: name,
                modulusLength: 2048,
                publicExponent: new Uint8Array([0x01, 0x00, 0x01]), // 65537
                hash: "SHA-256",
            }, true, keykeyUsages);
            // 导出公钥
            //这里spki意思就是Subject_Public_Key_Info 导出类型就是和C#里ImportSubjectPublicKeyInfo
            const publicKey = yield window.crypto.subtle.exportKey("spki", keyPair.publicKey);
            // 导出私钥
            const privateKey = yield window.crypto.subtle.exportKey("pkcs8", keyPair.privateKey);
            // 转换为Base64格式
            const rSAPublicKey = btoa(String.fromCharCode.apply(null, new Uint8Array(publicKey)));
            const rSAPrivateKey = btoa(String.fromCharCode.apply(null, new Uint8Array(privateKey)));
            // 存储到sessionStorage
            sessionStorage.setItem(`RSA${keyName}PublicKey`, rSAPublicKey);
            sessionStorage.setItem(`RSA${keyName}PrivateKey`, rSAPrivateKey);
            return {
                isSuccess: true,
                publicKey: rSAPublicKey,
                privateKey: rSAPrivateKey,
                message: null,
            };
        }
        catch (error) {
            return {
                isSuccess: false,
                publicKey: null,
                privateKey: null,
                message: "生成密钥对时出错",
            };
        }
    });
}
function getRSAHashAsync(arrayList) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 对数组进行排序
            arrayList.sort();
            // 将数组元素拼接成字符串
            const raw = arrayList.join('');
            // 使用SubtleCrypto API计算SHA256哈希
            const encoder = new TextEncoder();
            const data = encoder.encode(raw);
            return yield crypto.subtle.digest('SHA-256', data)
                .then(hash => {
                return {
                    isSuccess: true,
                    data: hash,
                    message: "Succss"
                };
            })
                .catch(error => {
                return {
                    isSuccess: false,
                    data: null,
                    message: '生成Hash值失败'
                };
            });
        }
        catch (error) {
            return Promise.resolve({
                isSuccess: false,
                data: null,
                message: '生成Hash值失败'
            });
        }
    });
}
function rsaSignDataAsync(hash, rsaSigningPrivateKey) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 检查私钥格式
            if (!rsaSigningPrivateKey || typeof rsaSigningPrivateKey !== 'string') {
                return {
                    isSuccess: false,
                    data: null,
                    message: "私钥格式错误：私钥为空或格式不正确"
                };
            }
            // 检查私钥是否为有效的Base64字符串
            try {
                // 将Base64私钥转换为ArrayBuffer
                const privateKeyBinary = Uint8Array.from(atob(rsaSigningPrivateKey), c => c.charCodeAt(0));
                // 导入私钥 - 使用RSA-PKCS1-v1_5算法进行签名
                const publicKey = yield window.crypto.subtle.importKey("pkcs8", privateKeyBinary, {
                    name: "RSASSA-PKCS1-v1_5",
                    hash: "SHA-256"
                }, false, ["sign"]);
                // 使用私钥签名
                const signature = yield window.crypto.subtle.sign("RSASSA-PKCS1-v1_5", publicKey, hash);
                // 将签名结果转换为Base64字符串
                const rsaSignData = btoa(String.fromCharCode.apply(null, new Uint8Array(signature)));
                return {
                    isSuccess: true,
                    data: rsaSignData,
                    message: "签名成功"
                };
            }
            catch (base64Error) {
                return {
                    isSuccess: false,
                    data: null,
                    message: "私钥Base64解码失败"
                };
            }
        }
        catch (error) {
            return {
                isSuccess: false,
                data: null,
                message: "RSA签名失败"
            };
        }
    });
}
//这里验证签名是用服务端的签名公钥
function rsaVerifyDataAsync(hash, signature, rsaSigningPrivateKey) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 检查参数
            if (!rsaSigningPrivateKey || typeof rsaSigningPrivateKey !== 'string') {
                return {
                    isSuccess: false,
                    data: null,
                    message: "公钥格式错误：公钥为空或格式不正确"
                };
            }
            try {
                // 将Base64私钥转换为ArrayBuffer
                const privateKeyBinary = Uint8Array.from(atob(rsaSigningPrivateKey), c => c.charCodeAt(0));
                // 导入公钥 - 使用RSASSA-PKCS1-v1_5算法进行验证
                const privateKey = yield window.crypto.subtle.importKey("spki", privateKeyBinary, {
                    name: "RSASSA-PKCS1-v1_5",
                    hash: "SHA-256"
                }, false, ["verify"]);
                const signatureBinary = Uint8Array.from(atob(signature), c => c.charCodeAt(0));
                // 使用公钥验证签名
                const isValid = yield window.crypto.subtle.verify("RSASSA-PKCS1-v1_5", privateKey, signatureBinary, hash);
                if (!isValid) {
                    return {
                        isSuccess: false,
                        data: null,
                        message: "签名验证失败"
                    };
                }
                return {
                    isSuccess: true,
                    data: null,
                    message: "签名验证成功"
                };
            }
            catch (base64Error) {
                return {
                    isSuccess: false,
                    data: null,
                    message: "Base64解码失败"
                };
            }
        }
        catch (error) {
            return {
                isSuccess: false,
                data: null,
                message: "签名验证失败"
            };
        }
    });
}
function rsaEncryptAsync(encryptContent, rsaEncryptionPublicKey) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 检查参数
            if (!encryptContent || typeof encryptContent !== 'string') {
                return {
                    isSuccess: false,
                    data: null,
                    message: "加密的数据不能为空"
                };
            }
            if (!rsaEncryptionPublicKey || typeof rsaEncryptionPublicKey !== 'string') {
                return {
                    isSuccess: false,
                    data: null,
                    message: "公钥格式错误：公钥为空或格式不正确"
                };
            }
            try {
                // 将Base64公钥转换为ArrayBuffer
                const publicKeyBinary = Uint8Array.from(atob(rsaEncryptionPublicKey), c => c.charCodeAt(0));
                // 导入公钥 - 使用RSA-OAEP算法进行验证
                const publicKey = yield window.crypto.subtle.importKey("spki", publicKeyBinary, {
                    name: "RSA-OAEP",
                    hash: "SHA-256"
                }, false, ["encrypt"]);
                // 将字符串转换为UTF-8编码的字节数组
                const encoder = new TextEncoder();
                const dataToEncrypt = encoder.encode(encryptContent);
                // 使用公钥加密
                const encrypted = yield window.crypto.subtle.encrypt("RSA-OAEP", publicKey, dataToEncrypt);
                // 将加密结果转换为Base64字符串
                const base64Encrypted = btoa(String.fromCharCode.apply(null, new Uint8Array(encrypted)));
                return {
                    isSuccess: true,
                    data: base64Encrypted,
                    message: "加密成功"
                };
            }
            catch (base64Error) {
                console.error('公钥导入错误');
                return {
                    isSuccess: false,
                    data: null,
                    message: "密钥导入失败"
                };
            }
        }
        catch (error) {
            console.error('RSA加密错误');
            return {
                isSuccess: false,
                data: null,
                message: "RSA加密失败"
            };
        }
    });
}
function rsaDecryptAsync(encryptContent, rsaEncryptionPrivateKey) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 检查参数
            if (!encryptContent || typeof encryptContent !== 'string') {
                return {
                    isSuccess: false,
                    data: null,
                    message: "加密数据格式错误：数据为空或格式不正确"
                };
            }
            if (!rsaEncryptionPrivateKey || rsaEncryptionPrivateKey.length === 0) {
                return {
                    isSuccess: false,
                    data: null,
                    message: "私钥格式错误：私钥为空或格式不正确"
                };
            }
            try {
                const privateKeyBinary = Uint8Array.from(atob(rsaEncryptionPrivateKey), c => c.charCodeAt(0));
                // 导入私钥 - 使用RSA-OAEP算法进行解密
                const privateKey = yield window.crypto.subtle.importKey("pkcs8", privateKeyBinary, {
                    name: "RSA-OAEP",
                    hash: "SHA-256"
                }, false, ["decrypt"]);
                const encryptBinary = Uint8Array.from(atob(encryptContent), c => c.charCodeAt(0));
                // 使用私钥解密
                const decrypted = yield window.crypto.subtle.decrypt("RSA-OAEP", privateKey, encryptBinary);
                // 将解密结果转换为字符串
                const decoder = new TextDecoder();
                const decryptedText = decoder.decode(decrypted);
                return {
                    isSuccess: true,
                    data: decryptedText,
                    message: "解密成功"
                };
            }
            catch (base64Error) {
                return {
                    isSuccess: false,
                    data: null,
                    message: "Base64解密失败"
                };
            }
        }
        catch (error) {
            return {
                isSuccess: false,
                data: null,
                message: "RSA解密失败"
            };
        }
    });
}
function getRSAKeyAsync(keyName) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            // 检查参数
            if (!keyName || typeof keyName !== 'string') {
                return {
                    isSuccess: false,
                    publicKey: null,
                    privateKey: null,
                    message: "密钥名称不能为空"
                };
            }
            // 存储到sessionStorage
            var rsaPublicKey = sessionStorage.getItem(`RSA${keyName}PublicKey`);
            var rsaPrivateKey = sessionStorage.getItem(`RSA${keyName}PrivateKey`);
            if (!rsaPublicKey || !rsaPrivateKey) {
                return {
                    isSuccess: false,
                    publicKey: null,
                    privateKey: null,
                    message: `${keyName}的密钥不存在，请先生成密钥对`
                };
            }
            return {
                isSuccess: true,
                publicKey: rsaPublicKey,
                privateKey: rsaPrivateKey,
                message: `${keyName}的密钥获取成功`
            };
        }
        catch (error) {
            return {
                isSuccess: false,
                publicKey: null,
                privateKey: null,
                message: `获取RSA密钥失败`
            };
        }
    });
}
$(function () {
    return __awaiter(this, void 0, void 0, function* () {
        yield initDefaultKeys();
    });
});
//# sourceMappingURL=Cryptography.js.map