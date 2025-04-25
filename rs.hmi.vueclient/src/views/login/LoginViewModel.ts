import { onMounted, onUnmounted, ref, watch } from 'vue'
import { LoginModel } from '../../Models/LoginModel';
import { ValidHelper } from '../../Commons/Helper/ValidHelper';
import type { IInputEvents } from '../../Interfaces/IInputEvents';
import type { IMessageEvents } from '../../Interfaces/IMessageEvents';
import { ViewModelBase } from '../../Models/ViewModelBase';
import { VerifyImgModel } from '../../Models/WebAPI/VerifyImgModel';
import { SimpleOperateResult } from '../../Commons/OperateResult/OperateResult';


export class LoginViewModel extends ViewModelBase {
  private loginModel = ref<LoginModel>(new LoginModel());

  //消息提示
  public RSMessageEvents: IMessageEvents | null = null;

  // 定义ref引用
  public RSEmailEvents: IInputEvents | null = null;
  public RSPasswordEvents: IInputEvents | null = null;
  public RSVerifyEvents: IInputEvents | null = null;

  public SliderBorderRef = ref<HTMLDivElement | null>(null);
  public BtnSliderRef = ref<HTMLButtonElement | null>(null);
  public VerifyImgHostRef = ref<HTMLDivElement | null>(null);
  public BtnImgSliderRef = ref<HTMLButtonElement | null>(null);

  constructor() {
    super();
    // 在构造函数中绑定全局事件
    this.bindGlobalEvents();
  }

  public get LoginModel(): LoginModel {
    return this.loginModel.value;
  }

  public set LoginModel(viewModel: LoginModel) {
    this.loginModel.value = viewModel;
  }

  // 绑定全局事件
  private bindGlobalEvents() {
    // 使用箭头函数确保this指向正确
    const handleGlobalMouseMove = (event: MouseEvent) => {
      this.HandleGlobalMouseMove(event);
    };

    const handleGlobalMouseUp = () => {
      this.HandleGlobalMouseUp();
    };

    const handleGlobalMouseleave = () => {
      this.HandleGlobalMouseleave();
    };

    // 添加全局事件监听
    window.addEventListener('mousemove', handleGlobalMouseMove);
    window.addEventListener('mouseup', handleGlobalMouseUp);
    window.addEventListener('mouseleave', handleGlobalMouseleave);
    onMounted(() => {
      this.InitBtnSliderControl();
    });
    // 使用 onUnmounted 钩子
    onUnmounted(() => {
      window.removeEventListener('mousemove', handleGlobalMouseMove);
      window.removeEventListener('mouseup', handleGlobalMouseUp);
      window.removeEventListener('mouseleave', handleGlobalMouseleave);
    });

    // 监听 ref 值的变化
    watch(
      () => this.LoginModel.BackgroundFillPercent,
      (newValue, oldValue) => {
        console.log('BackgroundFillPercent 发生变化:', newValue, oldValue);
      }
    );
  }




  public HandleRegister(): void {
    this.RouterUtil.Push('/Register')
  }

  public async HandleLogin(): Promise<void> {




    // 这里进行客户端简单的表单验证
    if (!this.ValidateForm()) {
      return;
    }
  }

  //private CanExecuteLogin(): boolean {
  //  const email = this.LoginModel.Email;
  //  const password = this.LoginModel.Password;
  //  const verify = this.LoginModel.Verify;
  //  // 只进行基本的非空检查，不触发消息和焦点设置
  //  return email != null
  //    && password != null
  //    && verify != null;
  //}

  public override  ValidateForm(): boolean {
    const email = this.LoginModel.Email;
    const password = this.LoginModel.Password;
    //const verify = this.LoginModel.Verify;


    if (!email || !ValidHelper.IsEmail(email)) {
      this.RSMessageEvents?.ShowWarningMsg('邮箱输入格式不正确');
      this.RSEmailEvents?.Focus();
      return false;
    }

    if (!password) {
      this.RSMessageEvents?.ShowWarningMsg('密码不能为空');
      this.RSPasswordEvents?.Focus();
      return false;
    }

    //if (!verify) {
    //  this.RSMessageEvents?.ShowWarningMsg('验证码不能为空');
    //  this.RSVerifyEvents?.Focus();
    //  return false;
    //}

    return true;
  }

  public HandleForgetPassword() {
    this.RouterUtil.Push("/Security");
  }


  private IsBtnSliderDragging = false;
  private BtnSliderStartX = 0;
  private BtnSliderHistoryPositionX = 0;
  private BtnSliderWidth = 0;
  private BtnSliderHeight = 0;
  private BtnSliderContainerWidth = 0;
  private BtnSliderContainerHeight = 0;
  private BtnSliderMaxPositionX = 0;







  public InitBtnSliderControl() {

    if (this.BtnSliderRef.value == null) {
      return;
    }

    if (this.SliderBorderRef.value == null) {
      return;
    }

    //获取滑块的长度和宽度
    this.BtnSliderWidth = this.BtnSliderRef.value.clientWidth
    this.BtnSliderHeight = this.BtnSliderRef.value.clientHeight;
    this.BtnSliderContainerWidth = this.SliderBorderRef.value.clientWidth;
    this.BtnSliderContainerHeight = this.SliderBorderRef.value.clientHeight;
    //获取最大移动距离
    this.BtnSliderMaxPositionX = this.BtnSliderContainerWidth - this.BtnSliderWidth;
  }


  public async InitVerifyControlAsync(): Promise<SimpleOperateResult> {
    //计算按钮的大小和宽度
    if (this.VerifyImgHostRef.value == null) {
      return SimpleOperateResult.CreateFailResult("无法获取图像容器");
    }

    if (this.BtnImgSliderRef.value == null) {
      return SimpleOperateResult.CreateFailResult("无法获取验证控件");
    }

    //做表单验证
    if (!this.ValidateForm()) {
      return SimpleOperateResult.CreateFailResult("表单验证不通过");
    }
   
    const getVerifyImgModelResult = await this.AxiosUtil.DecryptGet<VerifyImgModel>('/api/v1/Security/GetVerifyImgModel', VerifyImgModel);
    if (!getVerifyImgModelResult.IsSuccess) {
      return getVerifyImgModelResult;
    }
    let verifyImgModel = getVerifyImgModelResult.Data;
    if (verifyImgModel == null) {
      return SimpleOperateResult.CreateFailResult("未获取到验证码信息");
    }
    verifyImgModel = VerifyImgModel.GetBlobUrl(verifyImgModel);

    this.LoginModel.VerifyImgUrl = verifyImgModel?.VerifyImgUrl;
    this.LoginModel.ImgSliderUrl = verifyImgModel?.ImgSliderUrl;

    const verifyImgHostWidth = this.VerifyImgHostRef.value?.clientWidth;
    const verifyImgHostHeight = this.VerifyImgHostRef.value?.clientHeight;
    const widthScale = verifyImgHostWidth / verifyImgModel.ImgWidth;
    const heightScale = verifyImgHostHeight / verifyImgModel.ImgHeight;
    this.LoginModel.BtnImgSliderWidth = verifyImgModel.IconWidth * widthScale;
    this.LoginModel.BtnImgSliderHeight = verifyImgModel.IconHeight * heightScale;

    //获取滑块的长度和宽度
    this.BtnImgSliderWidth = this.BtnImgSliderRef.value.clientWidth
    this.BtnImgSliderHeight = this.BtnImgSliderRef.value.clientHeight;

    this.BtnImgSliderContainerWidth = this.VerifyImgHostRef.value.clientWidth;
    this.BtnImgSliderContainerHeight = this.VerifyImgHostRef.value.clientHeight;

    //获取最大移动距离
    this.BtnImgSliderMaxPositionX = this.BtnImgSliderContainerWidth - this.BtnImgSliderWidth;
    this.BtnImgSliderMaxPositionY = this.BtnImgSliderContainerHeight - this.BtnImgSliderHeight;

    //设置默认位置
    this.BtnImgSliderHistoryPositionX = verifyImgModel.IconBtnDefaultX * widthScale;
    this.BtnImgSliderHistoryPositionY = verifyImgModel.IconBtnDefaultY * heightScale;

    //先在这里设置个随机默认值
    this.LoginModel.BtnImgSliderPositionX = this.BtnImgSliderHistoryPositionX;
    this.LoginModel.BtnImgSliderPositionY = this.BtnImgSliderHistoryPositionY;

    return SimpleOperateResult.CreateSuccessResult();
  }

  public HandleGlobalMouseMove(event: MouseEvent) {
    if (this.IsBtnSliderDragging) {
      this.HandleBtnSliderMove(event);
    } else if (this.IsBtnImgSliderDragging) {
      this.HandleBtnImgSliderMove(event);
    }
  }

  //滑动块事件
  public async HandleBtnSliderMove(event: MouseEvent): Promise<void> {
    if (this.RSLoadingEvents == null) {
      return;
    }
    let newPositionX = this.BtnSliderHistoryPositionX + event.clientX - this.BtnSliderStartX;
    newPositionX = Math.min(newPositionX, this.BtnSliderMaxPositionX);
    newPositionX = Math.max(0, newPositionX);
    this.LoginModel.BtnSliderPositionX = newPositionX;
    this.LoginModel.BackgroundFillPercent = Math.floor((this.LoginModel.BtnSliderPositionX + this.BtnSliderWidth) / this.BtnSliderContainerWidth * 100);

    if (this.LoginModel.BackgroundFillPercent > 99 && !this.LoginModel.IsShowVerifyImg) {
      this.LoginModel.IsShowVerifyImg = true;
      //在这里发起获取验证码请求
      const getRegisterVerifyResult = await this.RSLoadingEvents.SimpleLoadingActionAsync(async () => {
        return await this.InitVerifyControlAsync();
      });
      //检查结果
      if (!getRegisterVerifyResult.IsSuccess) {
        this.RSMessageEvents?.ShowDangerMsg(getRegisterVerifyResult.Message);
        return;
      }
    } else if (this.LoginModel.IsShowVerifyImg && this.LoginModel.BackgroundFillPercent < 20) {
      this.LoginModel.IsShowVerifyImg = false;
    }
  }

  //处理验证码拖动按钮事件
  public HandleBtnImgSliderMove(event: MouseEvent) {
    let newPositionX = this.BtnImgSliderHistoryPositionX + event.clientX - this.BtnImgSliderStartX;
    newPositionX = Math.min(newPositionX, this.BtnImgSliderMaxPositionX);
    newPositionX = Math.max(0, newPositionX);
    this.LoginModel.BtnImgSliderPositionX = newPositionX;

    let newPositionY = this.BtnImgSliderHistoryPositionY + event.clientY - this.BtnImgSliderStartY;
    newPositionY = Math.min(newPositionY, this.BtnImgSliderMaxPositionY);
    newPositionY = Math.max(0, newPositionY);
    this.LoginModel.BtnImgSliderPositionY = newPositionY;
  }

  //处理滑动按钮鼠标按下事件
  public HandleBtnSliderMousedown(event: MouseEvent) {
    if (this.BtnSliderRef.value == null) {
      return;
    }
    if (this.SliderBorderRef.value == null) {
      return;
    }
    this.IsBtnSliderDragging = true;

    this.BtnSliderStartX = event.clientX;
  }


  //这是图像拖动按钮的变量
  private IsBtnImgSliderDragging = false;
  private BtnImgSliderStartX = 0;
  private BtnImgSliderStartY = 0;
  private BtnImgSliderHistoryPositionX = 0;
  private BtnImgSliderHistoryPositionY = 0;
  private BtnImgSliderWidth = 0;
  private BtnImgSliderHeight = 0;
  private BtnImgSliderContainerWidth = 0;
  private BtnImgSliderContainerHeight = 0;

  private BtnImgSliderMaxPositionX = 0;
  private BtnImgSliderMaxPositionY = 0;


  //图像拖动快鼠标按下事件
  public HandleBtnImgSliderMousedown(event: MouseEvent) {

    if (this.VerifyImgHostRef.value == null) {
      return;
    }
    if (this.BtnImgSliderRef.value == null) {
      return;
    }
    this.IsBtnImgSliderDragging = true;
    this.BtnImgSliderStartX = event.clientX;
    this.BtnImgSliderStartY = event.clientY;
  }

  //窗体鼠标弹起事件
  public HandleGlobalMouseUp() {
    this.IsBtnSliderDragging = false;
    this.IsBtnImgSliderDragging = false;
    this.BtnSliderHistoryPositionX = this.LoginModel.BtnSliderPositionX;
    this.BtnImgSliderHistoryPositionX = this.LoginModel.BtnImgSliderPositionX;
    this.BtnImgSliderHistoryPositionY = this.LoginModel.BtnImgSliderPositionY;
  }

  //处理鼠标离开
  public HandleGlobalMouseleave() {
    this.HandleGlobalMouseUp();
  }
} 
