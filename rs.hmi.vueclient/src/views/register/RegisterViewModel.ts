//import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Cryptography } from '../../Commons/Cryptography/Cryptography';
import { CommonUtils } from '../../Commons/Utils';
import type { IEvents } from '../../Interfaces/IEvents';
import { EmailRegisterPostModel } from '../../Models/EmailRegisterPostModel';
import Axios from '../../Commons/Axios';
import  { AESEncryptModel } from '../../Models/AESEncryptModel';
import  { GenericOperateResult } from '../../Commons/OperateResult/OperateResult';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import  { RegisterModel } from '../../Models/RegisterModel';
import { MessageModel } from '../../Models/MessageModel';
export class RegisterViewModel {
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;

  public RegisterModel: RegisterModel;
  public MessageModel: MessageModel;
  //public Email = ref('')
  //public Password = ref('')
  //public PasswordConfirm = ref('')
  //public Message = ref('')
  //public MessageType = ref('')

  private router = useRouter()
  public RSEmailRef: IEvents | null = null;
  public RSPasswordRef: IEvents | null = null;
  public RSPasswordConfirmRef: IEvents | null = null;


  constructor() {
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();
    //this.Message = this.CommonUtils.Message;
    //this.MessageType = this.CommonUtils.MessageType;
    this.RegisterModel = new RegisterModel();
    this.MessageModel = new MessageModel();
  }

  //public SetRSEmailRef(ref: IEvents) {
  //  this.RSEmailRef = ref;
  //}

  //public SetRSPasswordRef(ref: IEvents) {
  //  this.RSPasswordRef = ref;
  //}

  //public SetRSPasswordConfirmRef(ref: IEvents) {
  //  this.RSPasswordConfirmRef = ref;
  //}

  public async HandleRegisterNext(): Promise<void> {
    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return
    }

    //验证通过后 对密码进行加密处理
    const passwordSHA256HashCode = await this.Cryptography.GetSHA256HashCode(this.RegisterModel.PasswordConfirm);
    const emailRegisterPostModel = new EmailRegisterPostModel();

    emailRegisterPostModel.Email = this.RegisterModel.Email;
    emailRegisterPostModel.Password = passwordSHA256HashCode.Data;


    //使用AES密钥对数据进行加密
    //AES对称加密数据
    const aesEncryptResult = await this.Cryptography.AESEncryptSimple(emailRegisterPostModel);
    if (!aesEncryptResult.IsSuccess) {
      this.CommonUtils.ShowWarningMsg(aesEncryptResult.Message);
      return;
    }

    const result = await Axios.post<EmailRegisterPostModel, GenericOperateResult<AESEncryptModel>>('/api/v1/Register/GetEmailVerification', aesEncryptResult.Data);

    if (!result.IsSuccess || result.Data==null) {
      return;
    }
    //AES对称解密数据
    await this.Cryptography.AESDecryptSimple<AESEncryptModel>(result.Data);
    



  }

  private ValidateForm(): boolean {
    if (!this.RegisterModel.Email && !ValidHelper.IsEmail(this.RegisterModel.Email)) {
      this.CommonUtils.ShowWarningMsg('邮箱输入不正确');
      if (this.RSEmailRef) {
        this.RSEmailRef.Focus();
      }
      return false;
    }
    if (!this.RegisterModel.Password) {
      this.CommonUtils.ShowWarningMsg('请输入密码');
      if (this.RSPasswordRef) {
        this.RSPasswordRef.Focus();
      }
      return false
    }
    if (!this.RegisterModel.PasswordConfirm) {
      this.CommonUtils.ShowWarningMsg('请输入确认密码');
      if (this.RSPasswordConfirmRef) {
        this.RSPasswordConfirmRef.Focus();
      }
      return false
    }
    if (!(this.RegisterModel.Password === this.RegisterModel.PasswordConfirm)) {
      this.CommonUtils.ShowWarningMsg('2次密码输入不一致');
      if (this.RSPasswordConfirmRef) {
        this.RSPasswordConfirmRef.Focus();
      }
      return false
    }
    return true
  }
} 
