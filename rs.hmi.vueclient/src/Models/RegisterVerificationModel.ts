/**
 * 注册验证码返回值类
 */
export class SessionModel {
  /**
   * Token值
   */
  public Token: string | null = null;

  /**
   * 验证码有效时间 
   */
  public Verification: string | null = null;

}
