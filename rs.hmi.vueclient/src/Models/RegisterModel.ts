import { LoginModel } from "./LoginModel";

/**
 * 注册类
 */
export class RegisterModel extends LoginModel {
  /**
   * 确认
   */
  public PasswordConfirm: string | null = null;

}
