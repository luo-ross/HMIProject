import { ref } from 'vue'
import { LoginModel } from '../../Models/LoginModel';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IImgVerifyEvents } from '../../Interfaces/IImgVerifyEvents';
import { LoginValidModel } from '../../Models/WebAPI/LoginValidModel';
import { SimpleOperateResult } from '../../Commons/OperateResult/OperateResult';
import { LoginResultModel } from '../../Models/WebAPI/LoginResultModel';

export class LoginViewModel extends ViewModelBase {
  public LoginModel = ref<LoginModel>(new LoginModel());

  // 定义ref引用
  public EmailEvents = ref<IInputEvents>(); 
  public PasswordEvents= ref<IInputEvents>();
  public ImgVerifyEvents = ref<IImgVerifyEvents>();  
  constructor() {
    super();
  }

  //public get LoginModel(): LoginModel {
  //  return this.loginModel.value;
  //}

  //public set LoginModel(viewModel: LoginModel) {
  //  this.loginModel.value = viewModel;
  //}

  public HandleRegister(): void {
    this.RouterUtil.Push('/Register')
  }

  public async HandleLogin(): Promise<void> {

    // 这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }

    if (this.ImgVerifyEvents == null) {
      return;
    }

    //获取验证码信息
    const getImgVerifyResult = await this.ImgVerifyEvents.value?.GetImgVerifyResultAsync();
    if (getImgVerifyResult!=null&&!getImgVerifyResult.IsSuccess) {
      this.MessageEvents.value?.ShowDangerMsg(getImgVerifyResult.Message);
    }
    if (getImgVerifyResult!=null) {
      this.LoginModel.value.ImgVerifyResult = getImgVerifyResult.Data;
    }

    if (this.LoginModel.value.ImgVerifyResult == null
      || this.LoginModel.value.ImgVerifyResult.Verify == null
      || this.LoginModel.value.ImgVerifyResult.VerifySessionId == null
    ) {
      this.MessageEvents.value?.ShowDangerMsg("未获取到验证码");
      return;
    }


    if (this.LoadingEvents == null) {
      return;
    }

    //进行登录
    const loginResult = await this.LoadingEvents.value?.SimpleLoadingActionAsync(async () => {

      const loginValidModel = new LoginValidModel();
      loginValidModel.Email = this.LoginModel.value.Email;

      const getSHA256HashCodeResult = await this.Cryptography.GetSHA256HashCode(this.LoginModel.value.Password);
      if (!getSHA256HashCodeResult.IsSuccess) {
        return getSHA256HashCodeResult;
      }
      if (this.LoginModel.value.ImgVerifyResult == null) {
        return SimpleOperateResult.CreateFailResult("未获取到验证码");
      }

      loginValidModel.Password = getSHA256HashCodeResult.Data;
      loginValidModel.VerifySessionId = this.LoginModel.value.ImgVerifyResult.VerifySessionId;
      loginValidModel.Verify = this.LoginModel.value.ImgVerifyResult.Verify;

      const getloginResult = await this.AxiosUtil.AESEnAndDecryptPost<LoginValidModel, LoginResultModel>('/api/v1/Security/ValidLogin', loginValidModel, LoginResultModel);
      if (!getloginResult.IsSuccess) {
        return getloginResult;
      }
      const loginResult = getloginResult.Data;

      if (loginResult == null) {
        return SimpleOperateResult.CreateFailResult("登录失败!");
      }
      //获取新的会话
      const sessionModel = loginResult.SessionModel;
      if (sessionModel != null) {
        sessionModel.IsLogin = true;
      }
      sessionStorage.setItem('SessionModel', JSON.stringify(sessionModel))
      return SimpleOperateResult.CreateSuccessResult();
    });

    if (loginResult!=null&&!loginResult.IsSuccess) {
      this.MessageEvents.value?.ShowDangerMsg(loginResult.Message);
      return;
    }

    this.RouterUtil.Push("/Home");
  }

  public override  ValidateForm(): boolean {
    const email = this.LoginModel.value.Email;
    const password = this.LoginModel.value.Password;

    if (!email) {
      this.MessageEvents.value?.ShowWarningMsg('邮箱不能为空');
      this.EmailEvents.value?.Focus();
      return false;
    }
    if (!ValidHelper.IsEmail(email)) {
      this.MessageEvents.value?.ShowWarningMsg('邮箱输入格式不正确');
      this.EmailEvents.value?.Focus();
      return false;
    }

    if (!password) {
      this.MessageEvents.value?.ShowWarningMsg('密码不能为空');
      this.PasswordEvents.value?.Focus();
      return false;
    }

    if (password.length < 8) {
      this.MessageEvents.value?.ShowWarningMsg('密码长度至少8位');
      this.PasswordEvents.value?.Focus();
      return false;
    }

    return true;
  }

  public HandleForgetPassword() {
    this.RouterUtil.Push("/Security");
  }


  // 使用类属性初始化器
  public OnBtnSliderMousedown = async (): Promise<SimpleOperateResult> => {
    if (!this.ValidateForm()) {
      return SimpleOperateResult.CreateFailResult();
    }
    return SimpleOperateResult.CreateSuccessResult();
  }

  ////这里是子组件图像验证需要进行调用的事件
  //public async OnBtnSliderMousedown(): Promise<SimpleOperateResult> {
  //  // 这里进行客户端简单的表单验证
  //  if (!this.ValidateForm()) {
  //    return SimpleOperateResult.CreateFailResult();
  //  }
  //  return SimpleOperateResult.CreateSuccessResult();
  //}
} 
