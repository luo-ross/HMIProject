import { EmailVerifyModel } from '../../Models/EmailVerifyModel';
import { ViewModelBase } from '../../Models/ViewModelBase';
import { ref } from 'vue';
import { RegisterVerifyValidModel } from '../../Models/WebAPI/RegisterVerifyValidModel';
export class EmailVerifyViewModel extends ViewModelBase {
  private emailVerifyModel = ref<EmailVerifyModel>(new EmailVerifyModel());
  constructor() {
    super();
    this.InitializeComponent();
    this.StartCountdown();

    //简单做一些验证
    if (this.EmailVerifyModel.Email == null
      || this.EmailVerifyModel.Token == null
      || this.EmailVerifyModel.RegisterSessionId == null
      || this.Utils.IsTimestampExpired(this.EmailVerifyModel.ExpireTime, 2)
    ) {
      //如果通过验证则跳转到邮箱验证页面
      this.RouterUtil.Push('/Register');
      return;
    }

    //如果通过 重置Axios的Token 只有这个Token才能访问接口
    this.AxiosUtil.Token = this.EmailVerifyModel.Token;
  }
  public get EmailVerifyModel(): EmailVerifyModel {
    return this.emailVerifyModel.value;
  }
  public set EmailVerifyModel(viewModel: EmailVerifyModel) {
    this.emailVerifyModel.value = viewModel;
  }

  private StartCountdown() {
    const timer = setInterval(() => {
      const now = Date.now();
      let remainingSeconds = this.EmailVerifyModel.ExpireTime - now;
      if (remainingSeconds < 0) {
        remainingSeconds = 0;
      }
      this.EmailVerifyModel.RemainingSeconds = Math.floor(remainingSeconds / 1000);
      if (this.EmailVerifyModel.RemainingSeconds <= 0) {
        clearInterval(timer);
        this.RouterUtil.Push("/Register");
      }
    }, 1000);
  }

  public HandleBackLogin() {
    this.RouterUtil.Push("/login");
  }

  public async HandleVerfiyConfirm(): Promise<void> {

    this.EmailVerifyModel.IsRegisterSuccucess = true;


    //表单做一些验证
    if (!this.ValidateForm()) {
      return;
    }
    //如果验证码失效 跳转到注册页面
    if (this.Utils.IsTimestampExpired(this.EmailVerifyModel.ExpireTime, 2)) {
      this.RouterUtil.Push("/Register");
      return;
    }

    if (this.RSLoadingEvents == null) {
      return;
    }
    //在这里发起注册事件
    const verifyValidResult = await this.RSLoadingEvents.SimpleLoadingActionAsync(async () => {
      const registerVerifyValidModel = new RegisterVerifyValidModel();
      registerVerifyValidModel.RegisterSessionId = this.EmailVerifyModel.RegisterSessionId;
      registerVerifyValidModel.Verify = this.EmailVerifyModel.Verify;
      return await this.AxiosUtil.AESEncryptPost<RegisterVerifyValidModel>('/api/v1/Register/EmailVerifyValid', registerVerifyValidModel);
    });

    //验证结果
    if (!verifyValidResult.IsSuccess) {
      this.RSMessageEvents?.ShowWarningMsg(verifyValidResult.Message);
      return;
    }

    //如果成功则清除数据
    this.ClearData();

    this.EmailVerifyModel.IsRegisterSuccucess = true;
  }

  public InitializeComponent() {

    //获取邮箱
    this.EmailVerifyModel.Email = sessionStorage.getItem("RegisterVerifyModel.Email");
    //获取注册JWT
    this.EmailVerifyModel.Token = sessionStorage.getItem("RegisterVerifyModel.Token");
    //获取注册会话
    this.EmailVerifyModel.RegisterSessionId = sessionStorage.getItem("RegisterVerifyModel.RegisterSessionId");
    //获取验证码失效时间
    this.EmailVerifyModel.ExpireTime = Number(sessionStorage.getItem("RegisterVerifyModel.ExpireTime"));


  }

  //清除数据
  public ClearData() {
    sessionStorage.removeItem("RegisterVerifyModel.Email");
    sessionStorage.removeItem("RegisterVerifyModel.Token");
    sessionStorage.removeItem("RegisterVerifyModel.RegisterSessionId");
    sessionStorage.removeItem("RegisterVerifyModel.ExpireTime");
  }

  public override ValidateForm(): boolean {
    // 检查每个输入框是否有值
    for (let i = 0; i < this.EmailVerifyModel.VerifyList.length; i++) {
      if (!this.EmailVerifyModel.VerifyList[i]) {
        // 如果发现空输入框，设置焦点并显示提示
        this.EmailVerifyModel.RSInputList[i]?.focus();
        this.RSMessageEvents?.ShowWarningMsg('请输入完整的验证码');
        return false;
      }
    }

    // 所有输入框都有值，继续处理验证逻辑
    this.EmailVerifyModel.Verify = this.EmailVerifyModel.VerifyList.join('');
    if (this.EmailVerifyModel.Verify.length != 6) {
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
      const nextInput = this.EmailVerifyModel.RSInputList[index + 1];
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
        this.EmailVerifyModel.VerifyList[i] = char;
      });

      // 将焦点移到最后一个输入框
      if (this.EmailVerifyModel.RSInputList[5]) {
        this.EmailVerifyModel.RSInputList[5].focus();
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
        this.EmailVerifyModel.VerifyList[index] = '';
        return;
      }

      // 如果不是第一个输入框，则跳转到前一个输入框并清除内容
      if (index > 0) {
        const prevInput = this.EmailVerifyModel.RSInputList[index - 1];
        if (prevInput) {
          prevInput.focus();
          this.EmailVerifyModel.VerifyList[index - 1] = '';
        }
      }
    }
  }
} 
