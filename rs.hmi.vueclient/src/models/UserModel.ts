export interface UserInfo {
  username: string;
  password: string;
}

export class UserModel {
  private static instance: UserModel;
  private userInfo: UserInfo | null = null;

  private constructor() {}

  public static getInstance(): UserModel {
    if (!UserModel.instance) {
      UserModel.instance = new UserModel();
    }
    return UserModel.instance;
  }

  // 获取用户名
  public getUsername(): string {
    return this.userInfo?.username || '';
  }

  // 设置用户名
  public setUsername(username: string): void {
    if (this.userInfo) {
      this.userInfo.username = username;
    }
  }


  public async login(username: string, password: string): Promise<boolean> {
    try {
      // TODO: 这里添加实际的登录API调用
      // 模拟登录请求
      if (username === 'admin' && password === 'admin') {
        this.userInfo = { username, password };
        localStorage.setItem('token', 'dummy-token');
        return true;
      }
      return false;
    } catch (error) {
      console.error('登录失败:', error);
      return false;
    }
  }

  public logout(): void {
    this.userInfo = null;
    localStorage.removeItem('token');
  }

  public isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  public getUserInfo(): UserInfo | null {
    return this.userInfo;
  }
} 
