import type { SimpleOperateResult } from '../../Commons/OperateResult/OperateResult';
import { RegisterVerifyValidModel } from '../../Models/RegisterVerifyValidModel';
import { ViewModelBase } from '../../Models/ViewModelBase';
import { ref } from 'vue';
export class EmailVerifyViewModel extends ViewModelBase {
  public VerifyList = ref<string[]>(['', '', '', '', '', '']);
  public RSInputList!: HTMLInputElement[];
  public Email: string | null = null;
  public RegisterSessionId: string | null = null;
  public ExpireTime: number = 0;
  public Token: string | null = null;
  public Verify: string | null = null;
  constructor() {
    super();
    this.RSInputList = [...Array(6)].fill(null) as HTMLInputElement[];
    //获取邮箱
    this.Email = sessionStorage.getItem("RegisterVerifyModel.Email");
    //获取注册JWT
    this.Token = sessionStorage.getItem("RegisterVerifyModel.Token");
    //获取注册会话
    this.RegisterSessionId = sessionStorage.getItem("RegisterVerifyModel.RegisterSessionId");
    //获取验证码失效时间
    this.ExpireTime = Number(sessionStorage.getItem("RegisterVerifyModel.ExpireTime"));

    //简单做一些验证
    if (this.Email == null
      || this.Token == null
      || this.RegisterSessionId == null
      || this.CommonUtils.IsTimestampExpired(this.ExpireTime, 2)
    ) {
      //如果通过验证则跳转到邮箱验证页面
      this.RouterUtil.Push('/Register');
      return;
    }
    //如果通过 重置Axios的Token 只有这个Token才能访问接口
    this.AxiosUtil.Token = this.Token;
  }


  public async HandleVerfiyConfirm(): Promise<void> {

    //表单做一些验证
    if (!this.ValidateForm()) {
      return;
    }
    if (this.RSLoadingEvents == null) {
      return;
    }

    //在这里发起注册事件
    const verifyValidResult = await this.RSLoadingEvents.InvokeLoadingActionAsync<SimpleOperateResult>(async () => {
      const registerVerifyValidModel = new RegisterVerifyValidModel();
      registerVerifyValidModel.RegisterSessionId = this.RegisterSessionId;
      registerVerifyValidModel.Verify = this.Verify;
      return this.AxiosUtil.AESEncryptPost<RegisterVerifyValidModel, SimpleOperateResult>('/api/v1/Register/GetEmailVerify', registerVerifyValidModel);
    });

    //验证结果
    if (!verifyValidResult.IsSuccess) {
      this.RSMessageEvents?.ShowWarningMsg(verifyValidResult.Message);
      return;
    }
  }

  public override ValidateForm(): boolean {
    // 检查每个输入框是否有值
    for (let i = 0; i < this.VerifyList.value.length; i++) {
      if (!this.VerifyList.value[i]) {
        // 如果发现空输入框，设置焦点并显示提示
        this.RSInputList[i]?.focus();
        this.RSMessageEvents?.ShowWarningMsg('请输入完整的验证码');
        return false;
      }
    }
    // 所有输入框都有值，继续处理验证逻辑
    this.Verify = this.VerifyList.value.join('');
    if (this.Verify.length != 6) {
      this.RSMessageEvents?.ShowWarningMsg('请输入完整的验证码');
      return false;
    }
    return true;
  }

  public HandleInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    // 自动跳转到下一个输入框
    if (value && index < 5) {
      const nextInput = this.RSInputList[index + 1];
      if (nextInput) {
        nextInput.focus();
      }
    }
  }

  public HandlePaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pastedText = event.clipboardData?.getData('text') || '';

    if (pastedText.length >= 6) {
      const code = pastedText.slice(0, 6);

      // 填充所有输入框
      code.split('').forEach((char, i) => {
        this.VerifyList.value[i] = char;
      });

      // 将焦点移到最后一个输入框
      if (this.RSInputList[5]) {
        this.RSInputList[5].focus();
      }
    }
  }

  public HandleKeyDown(index: number, event: KeyboardEvent): void {
    // 处理退格键
    if (event.key === 'Backspace') {
      const input = event.target as HTMLInputElement;

      // 如果当前输入框有内容，先清除当前内容
      if (input.value) {
        input.value = '';
        this.VerifyList.value[index] = '';
        return;
      }

      // 如果不是第一个输入框，则跳转到前一个输入框并清除内容
      if (index > 0) {
        const prevInput = this.RSInputList[index - 1];
        if (prevInput) {
          prevInput.focus();
          this.VerifyList.value[index - 1] = '';
        }
      }
    }
  }
} 
