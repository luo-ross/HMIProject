import { ref } from 'vue'
import { MessageEnum } from './Enums/MessageEnum';

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

  /**
   * 生成随机数字字符串
   */
  public CreateRandCode(len: number): string {
    return Array.from({ length: len }, () =>
      Math.floor(Math.random() * 10)
    ).join('');
  }

  
  public ShowDangerMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Danger, msg);
  }

  public ShowDarkMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Dark, msg);
  }

  public ShowDismissibleMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Dismissible, msg);
  }

  public ShowHeadingMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Heading, msg);
  }


  public ShowInfoMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Info, msg);
  }


  public ShowLightMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Light, msg);
  }


  public ShowLinkMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Link, msg);
  }


  public ShowPrimaryMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Primary, msg);
  }


  public ShowSecondaryMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Secondary, msg);
  }


  public ShowSuccessMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Success, msg);
  }

  /**
   * 显示信息消息
   */
  public ShowWarningMsg(msg: string): void {
    this.ShowMsg(MessageEnum.Warning, msg);
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
  private ShowMsg(type: MessageEnum, msg: string): void {
    this.ClearMsg();
    this.Message.value = msg;
    this.MessageType.value = type;
    this.TimerId = setTimeout(() => {
      this.ClearMsg();
      this.TimerId = -1;
    }, 3000);
  }
}





