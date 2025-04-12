/**
 * 注册验证码类
 */
export class SessionModel {
  /**
   * 注册会话Id
   */
  public Token: string | null = null;

  /**
   * 验证码 
   */
  public Verification: string | null = null;

}
