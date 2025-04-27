import { ref } from 'vue'
import { LoginModel } from '../../Models/LoginModel';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IImgVerifyEvents } from '../../Interfaces/IImgVerifyEvents';
import { LoginValidModel } from '../../Models/WebAPI/LoginValidModel';
import { SimpleOperateResult } from '../../Commons/OperateResult/OperateResult';

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

    //获取验证码信息
    const getImgVerifyResult =await this.ImgVerifyEvents.GetImgVerifyResultAsync();
    if (!getImgVerifyResult.IsSuccess) {
      this.MessageEvents?.ShowDangerMsg(getImgVerifyResult.Message);
    }
    this.LoginModel.ImgVerifyResult = getImgVerifyResult.Data;
    if (this.LoginModel.ImgVerifyResult == null
      || this.LoginModel.ImgVerifyResult.MouseMovingTrack.length == 0
      || this.LoginModel.ImgVerifyResult.Verify == null
      || this.LoginModel.ImgVerifyResult.VerifySessionId == null
    ) {
      this.MessageEvents?.ShowDangerMsg("未获取到验证码");
    }


    if (this.LoadingEvents==null) {
      return;
    }

    //进行登录
    const loginResult = await this.LoadingEvents.SimpleLoadingActionAsync(async () => {

      const loginValidModel = new LoginValidModel();
      loginValidModel.UserName = this.LoginModel.Email;

      const getSHA256HashCodeResult =await this.Cryptography.GetSHA256HashCode(this.LoginModel.Password);
      if (!getSHA256HashCodeResult.IsSuccess) {
        return getSHA256HashCodeResult;
      }
      if (this.LoginModel.ImgVerifyResult==null) {
        return SimpleOperateResult.CreateFailResult("未获取到验证码");
      }

      loginValidModel.Password = getSHA256HashCodeResult.Data;
      loginValidModel.VerifySessionId = this.LoginModel.ImgVerifyResult.VerifySessionId ;
      loginValidModel.Verify = this.LoginModel.ImgVerifyResult.Verify;

      return await this.AxiosUtil.AESEncryptPost<LoginValidModel>('/api/v1/Security/ValidLogin', loginValidModel);
    });

    if (!loginResult.IsSuccess) {
      this.MessageEvents?.ShowDangerMsg(loginResult.Message);
      return;
    }

  }

  public override  ValidateForm(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;

    if (!email ) {
      this.MessageEvents?.ShowWarningMsg('邮箱不能为空');
      this.EmailEvents?.Focus();
      return false;
    }
    if (!ValidHelper.IsEmail(email)) {
      this.MessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.EmailEvents?.Focus();
      return false;
    }

    if (!password) {
      this.MessageEvents?.ShowWarningMsg('密码不能为空');
      this.PasswordEvents?.Focus();
      return false;
    }

    if (password.length<8) {
      this.MessageEvents?.ShowWarningMsg('密码长度至少8位');
      this.PasswordEvents?.Focus();
      return false;
    }

    return true;
  }

  public HandleForgetPassword() {
    this.RouterUtil.Push("/Security");
  }
} 
