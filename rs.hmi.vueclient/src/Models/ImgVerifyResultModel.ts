import type { IPoint } from "../Interfaces/IPoint";
import type { RectModel } from "./WebAPI/RectModel";

/**
 * 验证码结果类
 */
export class ImgVerifyResultModel {

  /**
  * 鼠标移动轨迹
  */
  public MouseMovingTrack: IPoint[]  = [];

  /**
  * 验证矩形框
  */
  public Rect: RectModel | null = null;

}


