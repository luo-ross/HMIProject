import type { RectModel } from "./RectModel";

/**
 * 登录验证类
 */
export class LoginValidModel {
  /**
   * 用户名
   */
  public Email: string | null = null;

  /**
   * 密码
   */
  public Password: string | null = null;

  /**
 * 验证会话
 */
  public VerifySessionId: string | null = null;

  /**
* 验证结果
*/
  public Verify: RectModel | null = null;

}    
