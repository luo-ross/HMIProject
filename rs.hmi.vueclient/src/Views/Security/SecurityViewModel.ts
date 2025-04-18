import { ref } from 'vue'
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { SecurityModel } from '../../Models/SecurityModel';
import { EmailSecurityModel } from '../../Models/EmailSecurityModel';


export class SecurityViewModel extends ViewModelBase {
  private securityModel = ref<SecurityModel>(new SecurityModel());
  public RSEmailEvents: IInputEvents | null = null;

  constructor() {
    super();

    this.SecurityModel.IsEmailSendSuccucess = true;
  }

  public get SecurityModel(): SecurityModel {
    return this.securityModel.value;
  }
  public set SecurityModel(viewModel: SecurityModel) {
    this.securityModel.value = viewModel;
  }

  //用户点击发送重置邮件事件
  public async HandlePasswordReset(): Promise<void> {

    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }

    if (this.RSLoadingEvents == null) {
      return;
    }

    //在这里发起注册事件
    const getRegisterVerifyResult = await this.RSLoadingEvents.SimpleLoadingActionAsync(async () => {
      const emailSecurityModel = new EmailSecurityModel();
      emailSecurityModel.Email = this.SecurityModel.Email;
      return await this.AxiosUtil.AESEncryptPost<EmailSecurityModel>('/api/v1/Security/EmailPasswordReset', emailSecurityModel);
    });

    //验证结果
    if (!getRegisterVerifyResult.IsSuccess) {
      this.RSMessageEvents?.ShowWarningMsg(getRegisterVerifyResult.Message);
      return;
    }

    this.SecurityModel.IsEmailSendSuccucess = true;
  }


  public HandleReturnSecurity() {
    this.SecurityModel.IsEmailSendSuccucess = false;
  }

  public override ValidateForm(): boolean {
    if (!this.SecurityModel.Email || !ValidHelper.IsEmail(this.SecurityModel.Email)) {
      this.RSMessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.RSEmailEvents?.Focus();
      return false;
    }
    return true
  }
} 
