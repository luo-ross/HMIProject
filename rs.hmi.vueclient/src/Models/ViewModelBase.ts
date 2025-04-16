import { Cryptography } from '../Commons/Cryptography/Cryptography';
import { CommonUtils } from '../Commons/Utils';
import type { ILoadingEvents } from '../Interfaces/ILoadingEvents';
import type { IMessageEvents } from '../Interfaces/IMessageEvents';
import { AxiosUtil } from '../Commons/Network/AxiosUtil';
import { RouterUtil } from '../Commons/Network/RouterUtil';

export abstract class ViewModelBase {
  public RouterUtil: RouterUtil;
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  public AxiosUtil: AxiosUtil;
  //加载
  public RSLoadingEvents: ILoadingEvents | null = null;
  //消息
  public RSMessageEvents: IMessageEvents | null = null;
  constructor() {
    this.CommonUtils = new CommonUtils();
    this.Cryptography = Cryptography.GetInstance();
    this.RouterUtil = RouterUtil.GetInstance();
    this.AxiosUtil = AxiosUtil.GetInstance();
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
