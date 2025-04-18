import { LogonModel } from "./LogonModel";
/**
 * 登录类
 */
export class LoginModel extends LogonModel {
 
  /**
  * 验证码
  */
  public Verify: string | null = null;
}


