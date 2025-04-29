import type { SessionModel } from "./SessionModel";

/**
 * 登录结果
 */
export class LoginResultModel {
  /**
   * 新的会话Model
   */
  public SessionModel: SessionModel | null = null;

  /**
   * 返回昵称
   */
  public NickName: string | null = null;

  /**
 * 返回用户的头像
 */
  public UserImgUrl: ArrayBuffer | null = null;

}    
