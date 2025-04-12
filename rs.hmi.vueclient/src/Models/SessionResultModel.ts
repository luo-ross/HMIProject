import type { SessionModel } from "./SessionModel";
import { SignModel } from "./SignModel";

/**
 * 返回给客户端会话签名数据
 */
export class SessionResultModel extends SignModel {
  /**
   * 服务端回传给客户端的加密公钥
   */
  public RSAEncryptPublicKey: string | null = null;

  /**
   * 服务端回传给客户端的签名公钥
   */
  public RSASignPublicKey: string | null = null;

  /**
   * 会话实体Token
   */
  public SessionModel: SessionModel | null = null;

}
