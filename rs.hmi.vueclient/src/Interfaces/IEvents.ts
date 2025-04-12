import type { Action } from "../Events/Action";

export interface IEvents {
  /**
  * 获取焦点
  */
  Focus: Action;

  /**
   * 失去焦点
   */
  Blur: Action;

  /**
   * 清空内容
   */
  Clear: Action;

  /**
   * 验证输入
   * @returns 是否验证通过
   */
  Validate: Action;
}    
