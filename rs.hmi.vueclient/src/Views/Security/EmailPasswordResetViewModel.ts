import { ref } from 'vue'
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { EmailPasswordResetModel } from '../../Models/EmailPasswordResetModel';
import { useRoute } from 'vue-router';
import { EmailPasswordConfirmModel } from '../../Models/WebAPI/EmailPasswordConfirmModel';

export class EmailPasswordResetViewModel extends ViewModelBase {
  private emailPasswordResetModel = ref<EmailPasswordResetModel>(new EmailPasswordResetModel());
  public PasswordEvents = ref<IInputEvents>();
  public PasswordConfirmEvents = ref<IInputEvents>(); 
  private route = useRoute();
  constructor() {
    super();

    this.InitQueryParams();
  }

  private InitQueryParams(): void {
    const email = this.route.query.Email as string;
    const token = this.route.query.Token as string;
    if (!email || !token) {
      this.RouterUtil.Push('/Login');
      return;
    }
    this.EmailPasswordResetModel.Email = email;
    this.EmailPasswordResetModel.Token = token;
  }

  public get EmailPasswordResetModel(): EmailPasswordResetModel {
    return this.emailPasswordResetModel.value;
  }

  public set EmailPasswordResetModel(viewModel: EmailPasswordResetModel) {
    this.emailPasswordResetModel.value = viewModel;
  }

  //用户点击发送重置邮件事件
  public async HandleEmailPasswordReset(): Promise<void> {

    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }

    if (this.LoadingEvents == null) {
      return;
    }

    //在这里发起注册事件
    const getRegisterVerifyResult = await this.LoadingEvents.value?.SimpleLoadingActionAsync(async () => {
      debugger;
      //验证通过后 对密码进行加密处理
      const passwordSHA256HashCode = await this.Cryptography.GetSHA256HashCode(this.EmailPasswordResetModel.PasswordConfirm);
      if (!passwordSHA256HashCode.IsSuccess) {
        return passwordSHA256HashCode;
      }
      const emailPasswordConfirmModel = new EmailPasswordConfirmModel();
      emailPasswordConfirmModel.Email = this.EmailPasswordResetModel.Email;
      emailPasswordConfirmModel.Password = passwordSHA256HashCode.Data;
      emailPasswordConfirmModel.Token = this.EmailPasswordResetModel.Token;
      return await this.AxiosUtil.AESEncryptPost<EmailPasswordConfirmModel>('/api/v1/Security/EmailPasswordResetConfirm', emailPasswordConfirmModel);
    });

    //验证结果
    if (getRegisterVerifyResult!=null&&!getRegisterVerifyResult.IsSuccess) {
      this.MessageEvents.value?.ShowWarningMsg(getRegisterVerifyResult.Message);
      return;
    }

    this.EmailPasswordResetModel.IsPasswordResetSuccucess = true;
  }


  public HandleRedirectToLoginView() {
    this.RouterUtil.Push("/Login");
  }

  public HandleBackSecurity() {
    this.RouterUtil.Push("/Security");
  }

  public override ValidateForm(): boolean {
    if (!this.EmailPasswordResetModel.Password) {
      this.MessageEvents.value?.ShowWarningMsg('请输入密码');
      this.PasswordEvents.value?.Focus();
      return false
    }

    if (!this.EmailPasswordResetModel.PasswordConfirm) {
      this.MessageEvents.value?.ShowWarningMsg('请输入确认密码');
      this.PasswordConfirmEvents.value?.Focus();
      return false
    }

    if (!(this.EmailPasswordResetModel.Password === this.EmailPasswordResetModel.PasswordConfirm)) {
      this.MessageEvents.value?.ShowWarningMsg('2次密码输入不一致');
      this.PasswordConfirmEvents.value?.Focus();
      return false
    }
    return true
  }
} 
