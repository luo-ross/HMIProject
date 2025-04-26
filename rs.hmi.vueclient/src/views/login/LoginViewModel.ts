import { ref, watch } from 'vue'
import { LoginModel } from '../../Models/LoginModel';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IImgVerifyEvents } from '../../Interfaces/IImgVerifyEvents';

export class LoginViewModel extends ViewModelBase {
  private loginModel = ref<LoginModel>(new LoginModel());

  // 定义ref引用
  public EmailEvents: IInputEvents | null = null;
  public PasswordEvents: IInputEvents | null = null;
  public ImgVerifyEvents: IImgVerifyEvents | null = null;
  constructor() {
    super();
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

    if (this.ImgVerifyEvents==null) {
      return;
    }

    const getImgVerifyResult =await this.ImgVerifyEvents.GetImgVerifyResultAsync();
    if (!getImgVerifyResult.IsSuccess) {
      this.MessageEvents?.ShowDangerMsg(getImgVerifyResult.Message);
    }
    const imgVerifyResult = getImgVerifyResult.Data;
    if (imgVerifyResult == null
      || imgVerifyResult.MouseMovingTrack.length == 0
      || imgVerifyResult.Rect == null
    ) {
      this.MessageEvents?.ShowDangerMsg("未获取到验证码");
    }
    console.log(getImgVerifyResult.Data);
  }

  public override  ValidateForm(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;

    if (!email || !ValidHelper.IsEmail(email)) {
      this.MessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.EmailEvents?.Focus();
      return false;
    }

    if (!password) {
      this.MessageEvents?.ShowWarningMsg('密码不能为空');
      this.PasswordEvents?.Focus();
      return false;
    }

    return true;
  }

  public HandleForgetPassword() {
    this.RouterUtil.Push("/Security");
  }
} 
