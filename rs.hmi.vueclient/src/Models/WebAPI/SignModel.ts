/**
 * 签名类
 */
export class SignModel {
  /**
   * 时间戳
   */
  public TimeStamp: string | null = null;

  /**
   * 随机数
   */
  public Nonce: string | null = null;

  /**
   * 签名
   */
  public MsgSignature: string | null = null;

}
