import { ref } from 'vue'
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { SecurityModel } from '../../Models/SecurityModel';
import { EmailSecurityModel } from '../../Models/WebAPI/EmailSecurityModel';


export class SecurityViewModel extends ViewModelBase {
  private securityModel = ref<SecurityModel>(new SecurityModel());
  public EmailEvents: IInputEvents | null = null;

  constructor() {
    super();
  }

  public get SecurityModel(): SecurityModel {
    return this.securityModel.value;
  }
  public set SecurityModel(viewModel: SecurityModel) {
    this.securityModel.value = viewModel;
  }

  //用户点击发送重置邮件事件
  public async HandSendEmailPasswordReset(): Promise<void> {

    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }

    if (this.LoadingEvents == null) {
      return;
    }

    //在这里发起注册事件
    const getRegisterVerifyResult = await this.LoadingEvents.SimpleLoadingActionAsync(async () => {
      const emailSecurityModel = new EmailSecurityModel();
      emailSecurityModel.Email = this.SecurityModel.Email;
      return await this.AxiosUtil.AESEncryptPost<EmailSecurityModel>('/api/v1/Security/PasswordResetEmailSend', emailSecurityModel);
    });

    //验证结果
    if (!getRegisterVerifyResult.IsSuccess) {
      this.MessageEvents?.ShowWarningMsg(getRegisterVerifyResult.Message);
      return;
    }

    this.SecurityModel.IsEmailSendSuccucess = true;
  }


  public HandleReturnSecurity() {
    this.SecurityModel.IsEmailSendSuccucess = false;
  }

  public HandleBackLogin() {
    this.RouterUtil.Push("/Login");
  }

  public override ValidateForm(): boolean {
    if (!this.SecurityModel.Email || !ValidHelper.IsEmail(this.SecurityModel.Email)) {
      this.MessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.EmailEvents?.Focus();
      return false;
    }
    return true
  }
} 
