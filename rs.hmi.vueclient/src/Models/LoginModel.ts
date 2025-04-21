import { LogonModel } from "./LogonModel";
/**
 * 登录类
 */
export class LoginModel extends LogonModel {

  /**
  * 验证码
  */
  public Verify: string | null = null;

  /**
* 这是滑块的位置X
*/
  public BtnSliderPositionX: number = 0;

  /**
* 这是滑动背景色百分比
*/
  public BackgroundFillPercent: number = 0;

  /**
* 图片滑动块宽度
*/
  public BtnImgSliderWidth: number = 0;

  /**
* 图片滑动块高度
*/
  public BtnImgSliderHeight: number = 0;


  /**
* 这是图像滑块的位置X
*/
  public BtnImgSliderPositionX: number = 0;

  /**
* 这是图像滑块的位置Y
*/
  public BtnImgSliderPositionY: number = 0;

  /**
* 是否显示验证图像
*/
  public IsShowVerifyImg: boolean = false;
  
}


