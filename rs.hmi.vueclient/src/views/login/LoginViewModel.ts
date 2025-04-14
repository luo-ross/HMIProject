import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { LoginModel } from '../../Models/LoginModel';
import { Cryptography } from '../../Commons/Cryptography/Cryptography';
import { CommonUtils } from '../../Commons/Utils';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IEvents } from '../../Interfaces/IEvents';
import { RelayCommand } from '../../Events/RelayCommand';
import { MessageModel } from '../../Models/MessageModel';
import type { IMessageEvents } from '../../Interfaces/IMessageEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';


export class LoginViewModel extends ViewModelBase {
  private loginModel = ref<LoginModel>(new LoginModel());
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  public MessageModel: MessageModel;
  public Router = useRouter()

  //消息提示
  public RSMessageEvents: IMessageEvents | null = null;

  // 定义ref引用
  public RSEmailEvents: IEvents | null = null;
  public RSPasswordEvents: IEvents | null = null;
  public RSVerifyEvents: IEvents | null = null;

  // 使用RelayCommand
  public LoginCommand: RelayCommand;
  public RegisterCommand: RelayCommand;

  constructor() {
    super();
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();
    this.MessageModel = new MessageModel();
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
    this.Router.push('/Register/Index')
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

  private ValidateForm(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;
    const verify = this.LoginModel.Verify;

   
    if (!email || !ValidHelper.IsEmail(email)) {
      this.RSMessageEvents?.ShowWarningMsg('邮箱输入不正确');
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
