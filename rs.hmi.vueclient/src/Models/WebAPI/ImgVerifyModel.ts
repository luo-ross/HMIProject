/**
 * 验证码图像
 */
export class ImgVerifyModel {

  /**
  * 验证会话Id
  */
  VerifySessionId: string | null = null;

  /**
   * 验证图像数据二合一
   */
  ImgBuffer: ArrayBuffer = new ArrayBuffer(0);

  /**
   * 背景图宽度
   */
  ImgWidth: number = 0;

  /**
   * 背景图高度
   */
  ImgHeight: number = 0;

  /**
   * 拖拽背景图片宽度
   */
  IconWidth: number = 0;

  /**
   * 拖拽背景图片高度
   */
  IconHeight: number = 0;

  /**
   * 拖拽按钮默认坐标位置X
   */
  ImgBtnPositionX: number = 0;

  /**
   * 拖拽按钮默认坐标位置Y
   */
  ImgBtnPositionY: number = 0;

  /**
   * 背景图
   */
  VerifyImgUrl: string | null = null;

  /**
   * 拖拽背景图
   */
  ImgSliderUrl: string | null = null;


  /**
   * 获取图片的Blob URL
   */
  public static GetBlobUrl(model: ImgVerifyModel) {

    const imgBuffer = new Uint8Array(model.ImgBuffer).buffer;
    // 读取第一个图片的长度（前8个字节）
    const dataView = new DataView(imgBuffer);
    const image1Length = dataView.getInt32(0, true); // true表示小端字节序

    // 分割两个图片的数据
    const image1Data = imgBuffer.slice(8, 8 + image1Length);
    const image2Data = imgBuffer.slice(8 + image1Length);

    // 创建两个Blob对象
    const blob1 = new Blob([image1Data], { type: 'image/jpeg' });
    const blob2 = new Blob([image2Data], { type: 'image/jpeg' });

    // 设置图片 URL
    model.VerifyImgUrl = URL.createObjectURL(blob1);
    model.ImgSliderUrl = URL.createObjectURL(blob2);

    return model;
  }

  /**
   * 释放Blob URL
   */
  public static ReleaseBlobUrl(model: ImgVerifyModel): void {
    if (model.VerifyImgUrl != null) {
      URL.revokeObjectURL(model.VerifyImgUrl);
    }

    if (model.ImgSliderUrl != null) {
      URL.revokeObjectURL(model.ImgSliderUrl);
    }
  }

}
