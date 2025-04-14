import { ref } from 'vue'
import { Cryptography } from '../../Commons/Cryptography/Cryptography';
import { CommonUtils } from '../../Commons/Utils';
import type { IEvents } from '../../Interfaces/IEvents';
import { EmailRegisterPostModel } from '../../Models/EmailRegisterPostModel';
import Axios from '../../Commons/Axios';
import { AESEncryptModel } from '../../Models/AESEncryptModel';
import { GenericOperateResult, SimpleOperateResult } from '../../Commons/OperateResult/OperateResult';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import { RegisterModel } from '../../Models/RegisterModel';
import type { IMessageEvents } from '../../Interfaces/IMessageEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';
import type { ILoadingEvents } from '../../Interfaces/ILoadingEvents';

export class RegisterViewModel extends ViewModelBase {
  private registerModel = ref<RegisterModel>(new RegisterModel());
  public Cryptography: Cryptography;
  public CommonUtils: CommonUtils;
  public RSEmailEvents: IEvents | null = null;
  public RSPasswordEvents: IEvents | null = null;
  public RSPasswordConfirmEvents: IEvents | null = null;
  public RSLoadingEvents: ILoadingEvents | null = null;
  //消息提示
  public RSMessageEvents: IMessageEvents | null = null;
  constructor() {
    super();
    this.Cryptography = Cryptography.GetInstance();
    this.CommonUtils = new CommonUtils();
  }

  public get RegisterModel(): RegisterModel {
    return this.registerModel.value;
  }
  public set RegisterModel(viewModel: RegisterModel) {
    this.registerModel.value = viewModel;
  }



  public async HandleRegisterNext(): Promise<void> {

    //这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return
    }

    //在这里发起注册事件
    await this.RSLoadingEvents?.InvokeLoadingActionAsync(async () => {

      await this.TaskDelay(5000);
      return SimpleOperateResult.CreateSuccessResult();

      //验证通过后 对密码进行加密处理
      const passwordSHA256HashCode = await this.Cryptography.GetSHA256HashCode(this.RegisterModel.PasswordConfirm);
      const emailRegisterPostModel = new EmailRegisterPostModel();

      emailRegisterPostModel.Email = this.RegisterModel.Email;
      emailRegisterPostModel.Password = passwordSHA256HashCode.Data;


      //使用AES密钥对数据进行加密
      //AES对称加密数据
      const aesEncryptResult = await this.Cryptography.AESEncryptSimple(emailRegisterPostModel);
      if (!aesEncryptResult.IsSuccess) {
        return SimpleOperateResult.CreateFailResult(aesEncryptResult);
      }

      const result = await Axios.post<EmailRegisterPostModel, GenericOperateResult<AESEncryptModel>>('/api/v1/Register/GetEmailVerification', aesEncryptResult.Data);

      if (!result.IsSuccess || result.Data == null) {
        return SimpleOperateResult.CreateFailResult("无法创建会话");
      }
      //AES对称解密数据
      const aesDecryptSimpleResult = await this.Cryptography.AESDecryptSimple<AESEncryptModel>(result.Data);

      if (!aesDecryptSimpleResult.IsSuccess) {
        return SimpleOperateResult.CreateFailResult(aesDecryptSimpleResult);
      }
      return SimpleOperateResult.CreateSuccessResult();
    });

    return;
  }

  private ValidateForm(): boolean {
    if (!this.RegisterModel.Email && !ValidHelper.IsEmail(this.RegisterModel.Email)) {
      this.CommonUtils.ShowWarningMsg('邮箱输入不正确');
      this.RSEmailEvents?.Focus();
      return false;
    }
    if (!this.RegisterModel.Password) {
      this.CommonUtils.ShowWarningMsg('请输入密码');
      this.RSPasswordEvents?.Focus();
      return false
    }
    if (!this.RegisterModel.PasswordConfirm) {
      this.CommonUtils.ShowWarningMsg('请输入确认密码');
      this.RSPasswordConfirmEvents?.Focus();
      return false
    }
    if (!(this.RegisterModel.Password === this.RegisterModel.PasswordConfirm)) {
      this.CommonUtils.ShowWarningMsg('2次密码输入不一致');
      this.RSPasswordConfirmEvents?.Focus();
      return false
    }
    return true
  }
} 
