import type { RectModel } from "./WebAPI/RectModel";

/**
 * 验证码结果类
 */
export class ImgVerifyResultModel {


  /**
  * 验证矩形框
  */
  public Verify: RectModel | null = null;


  /**
  * 验证会话Id
  */
  public VerifySessionId: string | null = null;

}


