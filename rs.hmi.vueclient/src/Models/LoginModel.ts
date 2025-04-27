import type { ImgVerifyResultModel } from "./ImgVerifyResultModel";
import { LogonModel } from "./LogonModel";
/**
 * 登录类
 */
export class LoginModel extends LogonModel {

  /**
 * 验证码数据
 */
  public ImgVerifyResult: ImgVerifyResultModel | null = null;
}


