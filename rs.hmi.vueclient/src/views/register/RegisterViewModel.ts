import { ref } from 'vue'
import { EmailRegisterPostModel } from '../../Models/WebAPI/EmailRegisterPostModel';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import { RegisterModel } from '../../Models/RegisterModel';
import { ViewModelBase } from '../../Models/ViewModelBase';
import { RelayCommand } from '../../Events/RelayCommand';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { RegisterVerifyModel } from '../../Models/WebAPI/RegisterVerifyModel';
import { GenericOperateResult } from '../../Commons/OperateResult/OperateResult';

export class RegisterViewModel extends ViewModelBase {
  private registerModel = ref<RegisterModel>(new RegisterModel());
  public EmailEvents = ref<IInputEvents>();
  public PasswordEvents = ref<IInputEvents>(); 
  public PasswordConfirmEvents = ref<IInputEvents>(); 

  // 使用RelayCommand
  public RegisterNextCommand: RelayCommand;

  constructor() {
    super();

    this.RegisterNextCommand = new RelayCommand(
      () => this.HandleRegisterNextAsync(),
      () => true
    );
  }


  public get RegisterModel(): RegisterModel {
    return this.registerModel.value;
  }
  public set RegisterModel(viewModel: RegisterModel) {
    this.registerModel.value = viewModel;
  }


  private async HandleRegisterNextAsync(): Promise<void> {



    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }


    if (this.LoadingEvents == null) {
      return;
    }

    //在这里发起注册事件
    const getRegisterVerifyResult = await this.LoadingEvents.value?.GenericLoadingActionAsync<RegisterVerifyModel>(async () => {
      //验证通过后 对密码进行加密处理
      const passwordSHA256HashCode = await this.Cryptography.GetSHA256HashCode(this.RegisterModel.PasswordConfirm);
      if (!passwordSHA256HashCode.IsSuccess) {
        return GenericOperateResult.CreateFailResult(passwordSHA256HashCode);
      }
      const emailRegisterPostModel = new EmailRegisterPostModel();
      emailRegisterPostModel.Email = this.RegisterModel.Email;
      emailRegisterPostModel.Password = passwordSHA256HashCode.Data;
      return this.AxiosUtil.AESEnAndDecryptPost<EmailRegisterPostModel, RegisterVerifyModel>('/api/v1/Register/GetEmailVerify', emailRegisterPostModel, RegisterVerifyModel);
    });

    //验证结果
    if (getRegisterVerifyResult!=null&&!getRegisterVerifyResult.IsSuccess) {
      this.MessageEvents.value?.ShowWarningMsg(getRegisterVerifyResult.Message);
      return;
    }

    const registerVerifyModel = getRegisterVerifyResult?.Data;
    if (registerVerifyModel == null) {
      this.MessageEvents.value?.ShowWarningMsg("未正确获取验证码");
      return;
    }

    if (registerVerifyModel.RegisterSessionId == null) {
      this.MessageEvents.value?.ShowWarningMsg("未正确获取验证码");
      return;
    }

    if (registerVerifyModel.Token == null) {
      this.MessageEvents.value?.ShowWarningMsg("未正确获取验证码");
      return;
    }

    if (this.Utils.IsTimestampExpired(registerVerifyModel.ExpireTime, 2)) {
      this.MessageEvents.value?.ShowWarningMsg("验证码已失效");
      return;
    }

    if (this.RegisterModel.Email != null) {
      sessionStorage.setItem("RegisterVerifyModel.Email", this.RegisterModel.Email);
    }

    sessionStorage.setItem("RegisterVerifyModel.RegisterSessionId", registerVerifyModel.RegisterSessionId);
    sessionStorage.setItem("RegisterVerifyModel.ExpireTime", registerVerifyModel.ExpireTime.toString());
    sessionStorage.setItem("RegisterVerifyModel.Token", registerVerifyModel.Token);

    //如果通过验证则跳转到邮箱验证页面
    this.RouterUtil.Push('/EmailVerify')
    return;
  }

  public HandleLogin(): void {
    this.RouterUtil.Push('/Login')
  }

  public override ValidateForm(): boolean {
    if (!this.RegisterModel.Email || !ValidHelper.IsEmail(this.RegisterModel.Email)) {
      this.MessageEvents.value?.ShowWarningMsg('邮箱输入格式不正确');
      this.EmailEvents.value?.Focus();
      return false;
    }

    if (!this.RegisterModel.Password) {
      this.MessageEvents.value?.ShowWarningMsg('请输入密码');
      this.PasswordEvents.value?.Focus();
      return false
    }

    if (!this.RegisterModel.PasswordConfirm) {
      this.MessageEvents.value?.ShowWarningMsg('请输入确认密码');
      this.PasswordConfirmEvents.value?.Focus();
      return false
    }

    if (!(this.RegisterModel.Password === this.RegisterModel.PasswordConfirm)) {
      this.MessageEvents.value?.ShowWarningMsg('2次密码输入不一致');
      this.PasswordConfirmEvents.value?.Focus();
      return false
    }

    return true
  }
} 
