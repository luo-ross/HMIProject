import { SignModel } from "./SignModel";

/**
 * 这是客户端向服务端发送会话请求类
 */
export class SessionRequestModel extends SignModel {
  /**
   * 签名验签公钥
   */
  public RSASignPublicKey: string | null = null;

  /**
   * 加解密公钥
   */
  public RSAEncryptPublicKey: string | null = null;

  /**
   * 客户端类型
   */
  public AudienceType: string | null = null;

}

