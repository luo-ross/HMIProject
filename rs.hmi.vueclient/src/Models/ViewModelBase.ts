export class ViewModelBase {
  constructor() {

  }

  // 定义一个函数来模拟延迟
  public async TaskDelay(ms: number): Promise<void> {
    return new Promise((resolve) => {
      setTimeout(resolve, ms);
    });
  }
}
