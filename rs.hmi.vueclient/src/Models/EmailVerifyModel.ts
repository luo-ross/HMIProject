/**
 * 邮箱注册信息类
 */
export class EmailVerifyModel {
  public IsRegisterSuccucess: boolean = false;
  public VerifyList: string[] ;
  public RSInputList: HTMLInputElement[];
  public Email: string | null=null ;
  public RegisterSessionId: string | null = null;
  public ExpireTime: number = 0;
  public Token: string | null = null;
  public Verify: string | null = null;
  public RemainingSeconds: number = 120;
  constructor() {
    this.VerifyList = [...Array(6)].fill("") as string[];
    this.RSInputList = [...Array(6)].fill(null) as HTMLInputElement[];
  }
 
}
