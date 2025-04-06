//import { ref } from 'vue'
//import { UserModel } from '../../models/UserModel'
//import type { UserInfo } from '../../models/UserModel'

//export class HomeViewModel {
//  private userModel: UserModel
  
//  public userInfo = ref<UserInfo | null>(null)

//  constructor() {
//    this.userModel = UserModel.getInstance()
//    this.userInfo.value = this.userModel.getUserInfo()
    
//    // 检查用户是否已登录，如果未登录则跳转到登录页
//    if (!this.userInfo.value) {
//      window.location.href = '/login.html'
//    }
//  }

//  public handleLogout(): void {
//    this.userModel.logout()
//    window.location.href = '/login.html'
//  }
//} 
