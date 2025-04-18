import { ref } from 'vue'
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import { EmailSecurityModel } from '../../Models/EmailEmailSecurityModel';
import { SimpleOperateResult } from '../../Commons/OperateResult/OperateResult';


export class SecurityViewModel extends ViewModelBase {
  private EmailSecurityModel = ref<EmailSecurityModel>(new EmailSecurityModel());
  public RSEmailEvents: IInputEvents | null = null;

  constructor() {
    super();
  }


  public get EmailSecurityModel(): EmailSecurityModel {
    return this.EmailSecurityModel.value;
  }
  public set EmailSecurityModel(viewModel: EmailSecurityModel) {
    this.EmailSecurityModel.value = viewModel;
  }

  //用户点击发送重置邮件事件
  private async HandlePasswordReset(): Promise<void> {

    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }

    if (this.RSLoadingEvents == null) {
      return;
    }

    //在这里发起注册事件
    const getRegisterVerifyResult = await this.RSLoadingEvents.SimpleLoadingActionAsync(async () => {

      const registerVerifyValidModel = new RegisterVerifyValidModel();
      registerVerifyValidModel.RegisterSessionId = this.EmailVerifyModel.RegisterSessionId;
      registerVerifyValidModel.Verify = this.EmailVerifyModel.Verify;
      return await this.AxiosUtil.AESEncryptPost<RegisterVerifyValidModel>('/api/v1/Register/EmailVerifyValid', registerVerifyValidModel);
      
    });

    //验证结果
    if (!getRegisterVerifyResult.IsSuccess) {
      this.RSMessageEvents?.ShowWarningMsg(getRegisterVerifyResult.Message);
      return;
    }
    
  
    return;
  }

  public override ValidateForm(): boolean {
    if (!this.EmailSecurityModel.Email || !ValidHelper.IsEmail(this.EmailSecurityModel.Email)) {
      this.RSMessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.RSEmailEvents?.Focus();
      return false;
    }
    return true
  }
} 
