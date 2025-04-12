/**
 * 对象扩展工具类
 */
export class ObjectExtensions {
  /**
   * 将对象转换为JSON字符串
   * @param obj 要转换的对象
   * @returns JSON字符串
   */
  public static ToJson<T extends object>(obj: T): string {
    return JSON.stringify(obj, (key, value) => {
      if (value instanceof Date) {
        return value.toISOString();
      }
      return value;
    });
  }

  /**
   * 根据指定格式将对象转换为JSON字符串
   * @param obj 要转换的对象
   * @param datetimeFormat 日期时间格式
   * @returns JSON字符串
   */
  public static ToJsonWithFormat<T extends object>(obj: T, datetimeFormat: string): string {
    return JSON.stringify(obj, (key, value) => {
      if (value instanceof Date) {
        const date = new Date(value);
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        return datetimeFormat
          .replace('yyyy', year.toString())
          .replace('MM', month)
          .replace('dd', day)
          .replace('HH', hours)
          .replace('mm', minutes)
          .replace('ss', seconds);
      }
      return value;
    });
  }

  /**
   * 将JSON字符串转换为指定类型的对象
   * @param json JSON字符串
   * @returns 转换后的对象
   */
  public static ToObject<T extends object>(json: string): T | null {
    if (!json) {
      return null;
    }
    try {
      return JSON.parse(json) as T;
    } catch (error) {
      console.error('JSON解析错误:', error);
      return null;
    }
  }

  /**
   * 将JSON字符串转换为对象
   * @param json JSON字符串
   * @returns 转换后的对象
   */
  public static ToJObject(json: string): Record<string, unknown> {
    if (!json) {
      return {};
    }
    try {
      return JSON.parse(json.replace(/&nbsp;/g, '')) as Record<string, unknown>;
    } catch (error) {
      console.error('JSON解析错误:', error);
      return {};
    }
  }

  /**
   * 将对象转换为查询字符串
   * @param entity 要转换的对象
   * @returns 查询字符串
   */
  public static ToQueryString<T extends Record<string, unknown>>(entity: T): string {
    if (!entity) {
      return '?';
    }
    const query: string[] = [];
    for (const [key, value] of Object.entries(entity)) {
      if (value !== undefined && value !== null) {
        query.push(`${key}=${encodeURIComponent(value.toString())}`);
      }
    }
    return query.length > 0 ? `?${query.join('&')}` : '?';
  }
} 
