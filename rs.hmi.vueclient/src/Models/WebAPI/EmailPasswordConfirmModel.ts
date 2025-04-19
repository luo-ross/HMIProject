import { LogonModel } from "../LogonModel";

/**
 * 邮箱密码确认请求类
 */
export class EmailPasswordConfirmModel extends LogonModel {
  /**
* 会话Id
*/
  public Token: string | null = null;

}
