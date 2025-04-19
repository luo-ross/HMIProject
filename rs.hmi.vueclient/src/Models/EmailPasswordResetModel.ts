import { LogonModel } from "./LogonModel";

/**
 * 邮箱密码重置
 */
export class EmailPasswordResetModel extends LogonModel  {
 
  /**
   * 会话ID
   */
  public Token: string | null = null;

  /**
  * 密码
  */
  public PasswordConfirm: string | null = null;

  /**
   * 密码是否重置成功
   */
  public IsPasswordResetSuccucess: boolean = false;

}
