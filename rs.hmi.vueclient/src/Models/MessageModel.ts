import { MessageEnum } from "../Commons/Enums/MessageEnum";

/**
 * 消息类
 */
export class MessageModel  {
  /**
   * 消息
   */
  public Message: string | null = null;

  /**
  * 消息类型
  */
  public MessageType: MessageEnum = MessageEnum.Success;
}
