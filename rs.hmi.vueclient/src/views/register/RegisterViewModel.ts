import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Cryptography } from '../../scripts/Cryptography'
import {
  CommonUtils,
} from '../../scripts/Utils'
import type { InputExpose } from '../../types/components'

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
  }

  private ValidateForm(): boolean {
    if (!this.Email.value && !this.CommonUtils.EmailValid(this.Email.value)) {
      this.CommonUtils.ShowWarningMsg('邮箱输入不正确');
      if (this.EmailInputRef) {
        this.EmailInputRef.Focus();
      }
      return false;
    }
    if (!this.Password.value ) {
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
      this.CommonUtils.ShowWarningMsg('请输入密码');
      if (this.PasswordConfirmInputRef) {
        this.PasswordConfirmInputRef.Focus();
      }
      return false
    }
    return true
  }
} 
