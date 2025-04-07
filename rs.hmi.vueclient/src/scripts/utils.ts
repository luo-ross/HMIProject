import { ref } from 'vue'

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
  Message: string;
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

// 定义消息类型
type MessageType = 'danger' | 'dark' | 'dismissible' | 'heading' | 'info' | 'light' | 'link' | 'primary' | 'secondary' | 'success' | 'warning';


/**
 * 通用工具类
 */
export class CommonUtils {
  public Message = ref('')
  public MessageType = ref('')

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

  ///**
  // * 发送POST请求
  // */
  //public async AjaxPost<TData, TResponse>(
  //  url: string,
  //  model: TData,
  //  success?: (result: TResponse) => Promise<OperateResult<Data>>,
  //  complete?: (response: Response | undefined) => Promise<OperateResult<Data>>,
  //  error?: (error: Error) => Promise<OperateResult<Data>>
  //): Promise<OperateResult<Data>> {
  //  let response;
  //  try {
  //    response = await fetch(url, {
  //      method: 'POST',
  //      headers: {
  //        'Content-Type': 'application/json'
  //      },
  //      body: JSON.stringify(model)
  //    });

  //    const data = await response.json() as TResponse;

  //    if (success) {
  //      return await success(data);
  //    }

  //    return {
  //      IsSuccess: true,
  //      Data: null,
  //      Message: ''
  //    };
  //  } catch (err) {
  //    if (error) {
  //      return await error(err as Error);
  //    }

  //    return {
  //      IsSuccess: false,
  //      Data: null,
  //      Message: '请求失败'
  //    };
  //  } finally {
  //    if (complete) {
  //      return await complete(response);
  //    }
  //  }
  //}

  /**
   * 生成随机数字字符串
   */
  public CreateRandCode(len: number): string {
    return Array.from({ length: len }, () =>
      Math.floor(Math.random() * 10)
    ).join('');
  }

  /**
   * 邮箱验证
   */
  public EmailValid(email: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return !this.IsEmptyOrNull(email) && emailRegex.test(email);
  }

  /**
   * 判断字符串是否为空
   */
  public IsEmptyOrNull(value: unknown): boolean {
    if (value === null || value === undefined) return true;
    if (typeof value === 'string') return value.trim() === '';
    return false;
  }

  /**
   * 显示错误消息
   */
  public ShowDangerMsg(msg: string): void {
    this.ShowMsg('danger', msg);
  }

  public ShowDarkMsg(msg: string): void {
    this.ShowMsg('dark', msg);
  }

  public ShowDismissibleMsg(msg: string): void {
    this.ShowMsg('dismissible', msg);
  }

  public ShowHeadingMsg(msg: string): void {
    this.ShowMsg('heading', msg);
  }


  public ShowInfoMsg(msg: string): void {
    this.ShowMsg('info', msg);
  }


  public ShowLightMsg(msg: string): void {
    this.ShowMsg('light', msg);
  }


  public ShowLinkMsg(msg: string): void {
    this.ShowMsg('link', msg);
  }


  public ShowPrimaryMsg(msg: string): void {
    this.ShowMsg('primary', msg);
  }


  public ShowSecondaryMsg(msg: string): void {
    this.ShowMsg('secondary', msg);
  }


  public ShowSuccessMsg(msg: string): void {
    this.ShowMsg('success', msg);
  }

  /**
   * 显示信息消息
   */
  public ShowWarningMsg(msg: string): void {
    this.ShowMsg('warning', msg);
  }


  /**
   * 清除消息
   */
  public ClearMsg(): void {
    this.Message.value = '';
    this.MessageType.value = '';
  }

  
  /**
   * 显示消息
   */
  private ShowMsg(type: MessageType, msg: string): void {
    this.ClearMsg();
    this.Message.value = msg;
    this.MessageType.value = type;
    this.TimerId = setTimeout(() => {
      this.ClearMsg();
      this.TimerId = -1;
    }, 3000);
  }

}




