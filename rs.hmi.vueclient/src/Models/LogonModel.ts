import { EmailModel } from "./WebAPI/EmailModel";

/**
 * 登录类
 */
export class LogonModel extends EmailModel  {

  /**
   * 密码
   */
  public Password: string | null = null;

}
