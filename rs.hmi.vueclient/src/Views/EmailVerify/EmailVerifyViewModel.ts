import { ViewModelBase } from '../../Models/ViewModelBase';
import { ref } from 'vue';

export class EmailVerifyViewModel extends ViewModelBase {
  public VerifyCode = ref<string[]>(['', '', '', '', '', '']);
  public RSInputList!: HTMLInputElement[];
  public Email: string | null = null;
  public RegisterSessionId: string | null = null;
  public ExpireTime: number = 0;
  constructor() {
    super();
    this.RSInputList = [...Array(6)].fill(null) as HTMLInputElement[];
    this.Email = sessionStorage.getItem("Email");
    this.RegisterSessionId = sessionStorage.getItem("RegisterSessionId");
    this.ExpireTime = Number(sessionStorage.getItem("ExpireTime"));

    if (this.Email == null
      || this.RegisterSessionId == null
      || this.CommonUtils.IsTimestampExpired(this.ExpireTime, 2)
    ) {
      //如果通过验证则跳转到邮箱验证页面
      this.Router.push('/Register');
      return;
    }
  }


  public HandleVerfiyConfirm(): void {
    if (!this.ValidateForm()) {
      return;
    }

  }

  public override ValidateForm(): boolean {
    // 检查每个输入框是否有值
    for (let i = 0; i < this.VerifyCode.value.length; i++) {
      if (!this.VerifyCode.value[i]) {
        // 如果发现空输入框，设置焦点并显示提示
        this.RSInputList[i]?.focus();
        this.RSMessageEvents?.ShowWarningMsg('请输入完整的验证码');
        return false;
      }
    }
    // 所有输入框都有值，继续处理验证逻辑
    const code = this.VerifyCode.value.join('');
    console.log('验证码:', code);

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
        this.VerifyCode.value[i] = char;
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
        this.VerifyCode.value[index] = '';
        return;
      }

      // 如果不是第一个输入框，则跳转到前一个输入框并清除内容
      if (index > 0) {
        const prevInput = this.RSInputList[index - 1];
        if (prevInput) {
          prevInput.focus();
          this.VerifyCode.value[index - 1] = '';
        }
      }
    }
  }
} 
