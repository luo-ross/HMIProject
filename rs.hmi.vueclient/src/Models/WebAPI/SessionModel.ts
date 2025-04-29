/**
 * 会话类
 */
export class SessionModel {
  /**
   * 加密后的AES 秘钥
   */
  public AesKey: string | null = null;

  /**
   * 会话ID 
   */
  public AppId: string | null = null;

  /**
   * 回传给客户端的Token
   */
  public Token: string | null = null;

  /**
  * 是否登录
  */
  public IsLogin: boolean  = false;

}
