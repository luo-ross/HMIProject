export interface SessionModel {
  AesKey: string | null;
  AppId: string | null;
  Token: string | null;
}

export interface Data {
  SessionModel: SessionModel;
  Data: string | null;
  RSASignPublicKey: string | null;
  RSAEncryptPublicKey: string | null;
  TimeStamp: string | null;
  Nonce: string | null;
  MsgSignature: string | undefined;
}

export interface ApiResponse<T = unknown> {
  isSuccess: boolean;
  data: T;
  message: string;
}


export interface OperateResultBase {
  IsSuccess: boolean;
  Message: string | null;
}

export interface OperateResult<T> extends OperateResultBase {
  Data: T | null;
}
export interface GernerateKeyResult extends OperateResultBase {
  PublicKey: string | null;
  PrivateKey: string | null;
}

export class SessionRequestModel {
  RSASignPublicKey: string | null = null;
  RSAEncryptPublicKey: string | null = null;
  Nonce: string | null = null;
  TimeStamp: string | null = null;
  MsgSignature: string | null = null;
  AudienceType: string | null = null;
};


export interface RequestConfig {
  url: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  headers?: Record<string, string>;
  data?: unknown;
}


/**
 * 通用工具类
 */
export class CommonUtils {
  private static timerId: number = -1;

  /**
   * 对象转JSON字符串
   */
  public static toJson<T>(obj: T): string {
    return JSON.stringify(obj);
  }

  /**
   * 获取URL查询参数
   */
  public static getQueryParam(name: string): string | null {
    const queryString = window.location.search.substring(1);
    const params = new URLSearchParams(queryString);
    return params.get(name);
  }

  /**
   * 发送POST请求
   */
  public static async ajaxPost<TData, TResponse>(
    url: string,
    model: TData,
    success?: (result: TResponse) => Promise<OperateResult<Data>>,
    complete?: (response: Response | undefined) => Promise<OperateResult<Data>>,
    error?: (error: Error) => Promise<OperateResult<Data>>
  ): Promise<OperateResult<Data>> {
    let response;
    try {
      response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(model)
      });

      const data = await response.json() as TResponse;

      if (success) {
        return await success(data);
      }

      return {
        IsSuccess: true,
        Data:null,
        Message: ''
      };
    } catch (err) {
      if (error) {
        return await error(err as Error);
      }

      return {
        IsSuccess: false,
        Data: null ,
        Message: '请求失败'
      };
    } finally {
      if (complete) {
        return await complete(response);
      }
    }
  }

  /**
   * 生成随机数字字符串
   */
  public static createRandCode(len: number): string {
    return Array.from({ length: len }, () =>
      Math.floor(Math.random() * 10)
    ).join('');
  }

  /**
   * 邮箱验证
   */
  public static emailValid(email: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return !this.isEmptyOrNull(email) && emailRegex.test(email);
  }

  /**
   * 判断字符串是否为空
   */
  public static isEmptyOrNull(value: unknown): boolean {
    if (value === null || value === undefined) return true;
    if (typeof value === 'string') return value.trim() === '';
    return false;
  }

  /**
   * 显示错误消息
   */
  public static showErrorMsg(msg: string|null): void {
    this.showMsg('error', msg);
  }

  /**
   * 显示信息消息
   */
  public static showInfoMsg(msg: string | null): void {
    this.showMsg('info', msg);
  }

  /**
   * 显示警告消息
   */
  public static showWarningMsg(msg: string | null): void {
    this.showMsg('warning', msg);
  }

  /**
   * 显示成功消息
   */
  public static showSuccessMsg(msg: string | null): void {
    this.showMsg('success', msg);
  }

  /**
   * 清除消息
   */
  public static clearMsg(): void {
    if (this.timerId > 0) {
      clearTimeout(this.timerId);
      this.timerId = -1;
    }

    const messageElement = document.querySelector('.error-message');
    if (messageElement) {
      messageElement.className = 'error-message d-none';
    }
  }

  /**
   * 显示消息
   */
  private static showMsg(type: MessageType, msg: string | null): void {
    const messageElement = document.querySelector('.error-message');
    if (!messageElement) return;

    if (this.timerId > 0) {
      clearTimeout(this.timerId);
      this.timerId = -1;
    }

    messageElement.className = `error-message alert-${type}`;
    messageElement.textContent = msg;

    this.timerId = window.setTimeout(() => {
      messageElement.className = 'error-message d-none';
      this.timerId = -1;
    }, 3000);
  }
}

// 定义消息类型
type MessageType = 'success' | 'error' | 'warning' | 'info';
