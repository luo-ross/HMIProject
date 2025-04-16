/**
 * 注册验证码返回值类
 */
export class RegisterVerifyModel {
  /**
   * 注册会话Id
   */
  public RegisterSessionId: string | null = null;

  /**
   * 验证码有效时间 
   */
  public ExpireTime: number  = 0;

  /**
  * 新的会话JWT
  */
  public Token: string | null = null;

}
