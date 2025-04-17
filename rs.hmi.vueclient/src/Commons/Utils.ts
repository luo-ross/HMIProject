/**
 * 通用工具类
 */
export class Utils {
  private TimerId: number = -1;

  /**
   * 对象转JSON字符串
   */
  public ToJson<T>(obj: T): string {
    return JSON.stringify(obj);
  }

  /**
   * 获取URL查询参数
   */
  public GetQueryParam(name: string): string | null {
    const queryString = window.location.search.substring(1);
    const params = new URLSearchParams(queryString);
    return params.get(name);
  }

  /**
   * 生成随机数字字符串
   */
  public CreateRandCode(len: number): string {
    return Array.from({ length: len }, () =>
      Math.floor(Math.random() * 10)
    ).join('');
  }


  /**
  * 验证时间戳是否失效
  * @param timestamp 时间戳
  * @param validDurationInMinutes 有效时间
  * @returns 是否有效
  */
  public IsTimestampExpired(timestamp: number, validDurationInMinutes: number): boolean {
    const currentTime = Date.now();
    const validDurationInMs = validDurationInMinutes * 60 * 1000;
    return currentTime - timestamp > validDurationInMs;
  }



}





