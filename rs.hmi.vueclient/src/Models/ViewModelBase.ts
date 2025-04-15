import { Cryptography } from '../Commons/Cryptography/Cryptography';
import { CommonUtils } from '../Commons/Utils';
import { useRouter } from 'vue-router'
import type { ILoadingEvents } from '../Interfaces/ILoadingEvents';
import type { IMessageEvents } from '../Interfaces/IMessageEvents';

export abstract class ViewModelBase {
  public Router = useRouter();
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  //加载
  public RSLoadingEvents: ILoadingEvents | null = null;
  //消息
  public RSMessageEvents: IMessageEvents | null = null;
  constructor() {
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();
  }

  // 定义一个函数来模拟延迟
  public async TaskDelay(ms: number): Promise<void> {
    return new Promise((resolve) => {
      setTimeout(resolve, ms);
    });
  }

  public ValidateForm(): boolean {
    return true;
  }
}
