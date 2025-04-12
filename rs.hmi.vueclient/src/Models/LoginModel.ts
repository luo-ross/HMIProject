import { SecurityModel } from "./SecurityModel";
/**
 * 登录类
 */
export class LoginModel extends SecurityModel {
 
  /**
  * 验证码
  */
  public Verify: string | null = null;
}


