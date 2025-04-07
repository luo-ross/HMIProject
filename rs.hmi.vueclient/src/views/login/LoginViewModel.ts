import { ref } from 'vue'
import { UserModel } from '../../models/UserModel'
import { Cryptography } from '../../scripts/Cryptography'
import {
  CommonUtils,
} from '../../scripts/Utils'
export class LoginViewModel {
  private UserModel: UserModel
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  //邮箱
  public Email = ref('')
  public Password = ref('')
  public Verify = ref('')
  public Loading = ref(false)
  public Message;
  public MessageType;
  constructor() {
    this.UserModel = UserModel.getInstance();
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();
    this.Message = this.CommonUtils.Message;
    this.MessageType = this.CommonUtils.MessageType;
    this.Init();
  }

  public async Init() {

    



    await this.Cryptography.InitDefaultKeys();
  }

  public async HandleLogin(): Promise<void> {

    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return
    }
  }

  private ValidateForm(): boolean {
    if (!this.Email.value && !this.CommonUtils.EmailValid(this.Email.value)) {
      this.CommonUtils.ShowWarningMsg('邮箱输入不正确');
      return false;
    }
    if (!this.Password.value || this.Password.value === ' ' || this.Password.value.length < 8) {
      this.CommonUtils.ShowWarningMsg('请输入密码');
      return false
    }
    if (!this.Verify.value) {
      this.CommonUtils.ShowWarningMsg('请输入验证码');
      return false
    }
    return true
  }



} 
