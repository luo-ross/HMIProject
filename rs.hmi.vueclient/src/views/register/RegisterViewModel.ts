import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Cryptography, AESEncryptModel, EmailRegisterPostModel } from '../../scripts/Cryptography'
import {
  CommonUtils,
  type OperateResult,
} from '../../scripts/Utils'
import type { InputExpose } from '../../types/components'
import WebApi from '../../scripts/AxiosConfig'
export class RegisterViewModel {
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;

  public Email = ref('')
  public Password = ref('')
  public PasswordConfirm = ref('')
  public Message = ref('')
  public MessageType = ref('')
  private router = useRouter()
  private EmailInputRef: InputExpose | null = null;
  private PasswordInputRef: InputExpose | null = null;
  private PasswordConfirmInputRef: InputExpose | null = null;

  constructor() {
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();
    this.Message = this.CommonUtils.Message;
    this.MessageType = this.CommonUtils.MessageType;
  }

  public SetEmailInputRef(ref: InputExpose) {
    this.EmailInputRef = ref;
  }

  public SetPasswordInputRef(ref: InputExpose) {
    this.PasswordInputRef = ref;
  }

  public SetPasswordConfirmInputRef(ref: InputExpose) {
    this.PasswordConfirmInputRef = ref;
  }

  public async HandleRegisterNext(): Promise<void> {
    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return
    }

    //验证通过后 对密码进行加密处理
    const passwordSHA256HashCode = await this.Cryptography.GetSHA256HashCode(this.PasswordConfirm.value);
    const emailRegisterPostModel = new EmailRegisterPostModel();

    emailRegisterPostModel.Email = this.Email.value;
    emailRegisterPostModel.Password = passwordSHA256HashCode.Data;


    //使用AES密钥对数据进行加密
    //AES对称加密数据
    const aesEncryptResult = await this.Cryptography.AESEncryptSimple(emailRegisterPostModel);
    if (!aesEncryptResult.IsSuccess) {
      this.CommonUtils.ShowWarningMsg(aesEncryptResult.Message);
      return;
    }

    const result = await WebApi.post<EmailRegisterPostModel, OperateResult<AESEncryptModel>>('/api/v1/Register/GetEmailVerification', emailRegisterPostModel);

    if (!result.IsSuccess || result.Data==null) {
      return;
    }
    //AES对称解密数据
    await this.Cryptography.AESDecryptSimple<AESEncryptModel>(result.Data);
    



  }

  private ValidateForm(): boolean {
    if (!this.Email.value && !this.CommonUtils.EmailValid(this.Email.value)) {
      this.CommonUtils.ShowWarningMsg('邮箱输入不正确');
      if (this.EmailInputRef) {
        this.EmailInputRef.Focus();
      }
      return false;
    }
    if (!this.Password.value) {
      this.CommonUtils.ShowWarningMsg('请输入密码');
      if (this.PasswordInputRef) {
        this.PasswordInputRef.Focus();
      }
      return false
    }
    if (!this.PasswordConfirm.value) {
      this.CommonUtils.ShowWarningMsg('请输入确认密码');
      if (this.PasswordConfirmInputRef) {
        this.PasswordConfirmInputRef.Focus();
      }
      return false
    }
    if (!(this.Password.value === this.PasswordConfirm.value)) {
      this.CommonUtils.ShowWarningMsg('2次密码输入不一致');
      if (this.PasswordConfirmInputRef) {
        this.PasswordConfirmInputRef.Focus();
      }
      return false
    }
    return true
  }
} 
