import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { LoginModel } from '../../Models/LoginModel';
import { Cryptography } from '../../Commons/Cryptography/Cryptography';
import { CommonUtils } from '../../Commons/Utils';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IEvents } from '../../Interfaces/IEvents';
import { RelayCommand } from '../../Events/RelayCommand';

export class LoginViewModel {
  private LoginModel: LoginModel
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  public Router = useRouter()

  // 定义ref引用
  public RSEmailEvnets: IEvents | null = null;
  public RSPasswordEvents: IEvents | null = null;
  public RSVerifyEvents: IEvents | null = null;

  // 使用RelayCommand
  public LoginCommand: RelayCommand;
  public RegisterCommand: RelayCommand;

  constructor() {
    //初始化属性
    this.LoginModel = new LoginModel();
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();

    // 初始化命令
    this.LoginCommand = new RelayCommand(
      () => this.HandleLogin(),
      () => this.CanExecuteLogin()
    );

    this.RegisterCommand = new RelayCommand(
      () => this.HandleRegister(),
      () => true
    );
  }

  public HandleRegister(): void {
    this.Router.push('/Register/Index')
  }

  public async HandleLogin(): Promise<void> {
    // 这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }
  }

  private CanExecuteLogin(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;
    const verify = this.LoginModel.Verify;

    // 只进行基本的非空检查，不触发消息和焦点设置
    return email != null
      && password != null
      && verify != null
      && email.length > 0
      && password.length > 0
      && verify.length > 0;
  }

  private ValidateForm(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;
    const verify = this.LoginModel.Verify;

    if (!email || !ValidHelper.IsEmail(email)) {
      this.CommonUtils.ShowWarningMsg('邮箱输入不正确');
      this.RSEmailEvnets?.Focus();
      return false;
    }

    if (!password) {
      this.CommonUtils.ShowWarningMsg('密码不能为空');
      this.RSPasswordEvents?.Focus();
      return false;
    }

    if (!verify) {
      this.CommonUtils.ShowWarningMsg('验证码不能为空');
      this.RSVerifyEvents?.Focus();
      return false;
    }

    return true;
  }
} 
