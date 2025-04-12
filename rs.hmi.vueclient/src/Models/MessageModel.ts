import  { MessageType } from "../Commons/Enums/MessageType";

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
  public MessageType: MessageType = MessageType.Success;
}
