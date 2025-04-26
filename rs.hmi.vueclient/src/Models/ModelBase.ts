import { Cryptography } from '../Commons/Cryptography/Cryptography';
import { Utils } from '../Commons/Utils';
import { AxiosUtil } from '../Commons/Network/AxiosUtil';
import { RouterUtil } from '../Commons/Network/RouterUtil';

export abstract class ModelBase {
  public RouterUtil: RouterUtil;
  public Cryptography: Cryptography;
  public Utils: Utils;
  public AxiosUtil: AxiosUtil;
 
  constructor() {
    this.Utils = new Utils();
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
