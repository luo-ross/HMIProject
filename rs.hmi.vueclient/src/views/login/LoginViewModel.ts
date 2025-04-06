import { ref } from 'vue'
import { UserModel } from '../../models/UserModel'
import { cryptography } from '../../scripts/cryptography'
export class LoginViewModel {
  private userModel: UserModel
  public cryptography: cryptography;

  public email = ref('')
  public password = ref('')
  public verify = ref('')
  public token = ref('')
  public loading = ref(false)
  public errorMessage = ref('')


  constructor() {
    this.userModel = UserModel.getInstance();
    this.cryptography = cryptography.getInstance();
    this.Init();
  }

  public async Init() {
    await this.cryptography.initDefaultKeys();
  }

  public async handleLogin(): Promise<void> {
    debugger

    this.email.value = "123123123123123";
    if (!this.validateForm()) {
      return
    }

    //this.loading.value = true
    //this.errorMessage.value = ''

    //try {
    //  // 使用邮箱或用户名登录
    //  const loginId = this.email.value || this.username.value

    //  const success = await this.userModel.login(
    //    loginId,
    //    this.password.value
    //  )

    //  if (success) {
    //    // 登录成功后跳转到home.html
    //    window.location.href = '/home.html'
    //  } else {
    //    this.errorMessage.value = '用户名/邮箱或密码错误'
    //  }
    //} catch (error) {
    //  this.errorMessage.value = '登录失败,请重试'
    //  console.error('登录失败:', error)
    //} finally {
    //  this.loading.value = false
    //}
  }

  private validateForm(): boolean {
    //if (!this.username.value && !this.email.value) {
    //  this.errorMessage.value = '请输入用户名或邮箱'
    //  return false
    //}
    //if (!this.password.value) {
    //  this.errorMessage.value = '请输入密码'
    //  return false
    //}
    //if (!this.verifyCode.value) {
    //  this.errorMessage.value = '请输入验证码'
    //  return false
    //}
    //if (!this.verifyToken.value) {
    //  this.errorMessage.value = '请输入验证令牌'
    //  return false
    //}
    return true
  }
} 
