import { ref } from 'vue'
import { LoginModel } from '../../Models/LoginModel';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { RelayCommand } from '../../Events/RelayCommand';
import type { IMessageEvents } from '../../Interfaces/IMessageEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';

export class LoginViewModel extends ViewModelBase {
  private loginModel = ref<LoginModel>(new LoginModel());

  //消息提示
  public RSMessageEvents: IMessageEvents | null = null;

  // 定义ref引用
  public RSEmailEvents: IInputEvents | null = null;
  public RSPasswordEvents: IInputEvents | null = null;
  public RSVerifyEvents: IInputEvents | null = null;

  // 使用RelayCommand
  public LoginCommand: RelayCommand;
  public RegisterCommand: RelayCommand;

  constructor() {
    super();

    // 初始化命令
    this.LoginCommand = new RelayCommand(
      () => this.HandleLogin(),
      () => true
    );

    this.RegisterCommand = new RelayCommand(
      () => this.HandleRegister(),
      () => true
    );
  }

  public get LoginModel(): LoginModel {
    return this.loginModel.value;
  }
  public set LoginModel(viewModel: LoginModel) {
    this.loginModel.value = viewModel;
  }

  public HandleRegister(): void {
    this.RouterUtil.Push('/Register')
  }

  public async HandleLogin(): Promise<void> {
    // 这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }
  }

  //private CanExecuteLogin(): boolean {
  //  const email = this.LoginModel.Email;
  //  const password = this.LoginModel.Password;
  //  const verify = this.LoginModel.Verify;
  //  // 只进行基本的非空检查，不触发消息和焦点设置
  //  return email != null
  //    && password != null
  //    && verify != null;
  //}

  public override  ValidateForm(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;
    const verify = this.LoginModel.Verify;

   
    if (!email || !ValidHelper.IsEmail(email)) {
      this.RSMessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.RSEmailEvents?.Focus();
      return false;
    }

    if (!password) {
      this.RSMessageEvents?.ShowWarningMsg('密码不能为空');
      this.RSPasswordEvents?.Focus();
      return false;
    }

    if (!verify) {
      this.RSMessageEvents?.ShowWarningMsg('验证码不能为空');
      this.RSVerifyEvents?.Focus();
      return false;
    }

    return true;
  }
} 
