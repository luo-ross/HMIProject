<style scoped>
  .slider-border {
    display: flex;
    width: 100%;
    border: 1px solid #198cff;
    border-radius: 5px;
    padding: 0;
    position: relative;
    height: 40px;
    background-color: #f5f5f5;
    margin: 2px 3px;
  }

  .btn-slider {
    background-color: #198cff;
    border-radius: 3px;
    padding: 0;
    border: none;
    outline: none;
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 0;
    height: 100%;
    cursor: grab;
    user-select: none;
    padding: 6px 10px;
  }

    .btn-slider:active, .btn-imgslider:active {
      cursor: grabbing;
    }

  .btn-slider-icon {
    width: 15px;
    height: 15px;
  }

  .btn-slider-icon-path {
    fill: #fff;
  }

  .slider-text {
    position: absolute;
    width: 100%;
    text-align: center;
    line-height: 40px;
    color: #666;
    pointer-events: none;
  }

  .background-fill {
    position: absolute;
    height: 100%;
    background: #198cff;
    opacity: 0.2;
  }

  .btn-slider:hover, .btn-imgslider:hover {
    opacity: 1;
  }

  .verify-img-host {
    width: 100%;
    height: 150px;
    position: absolute;
    top: -155px;
    border-radius: 5px;
    background-size: 100% 100%;
    background-repeat: no-repeat;
    background-position: center;
    overflow: hidden;
  }

  .btn-imgslider {
    width: 50px;
    height: 50px;
    background-image: url(/public/icon.png);
    background-size: cover;
    background-position: center;
    border: none;
    position: absolute;
    left: 50px;
    top: 45px;
    box-shadow: 0px 0px 20px rgba(255,0,0,1);
    background-color: transparent;
  }

  .d-none {
    display: none;
  }
</style>

<template>
  <div class="form-row" ref="ElementRef">
    <div ref="SliderBorderRef"
         class="slider-border">
      <span class="slider-text">向右滑动滑块触发图像验证</span>
      <div :style="{width: state.BackgroundFillPercent+'%' }"
           class="background-fill">
      </div>
      <button ref="BtnSliderRef"
              :style="{left:state.BtnSliderPositionX+'px' }"
              class="btn-slider"
              @mousedown="HandleBtnSliderMousedown">
        <svg t="1745157586510" class="icon btn-slider-icon"
             viewBox="0 0 1024 1024"
             version="1.1"
             xmlns="http://www.w3.org/2000/svg"
             p-id="2693"
             width="64"
             height="64">
          <path class="btn-slider-icon-path"
                d="M567.32505 547.18536c20.970614-21.479197 20.970614-56.307424 0-77.790714L185.251168 77.115332c-20.971637-21.47715-54.975079-21.47715-75.948763 0-20.973684 21.484314-20.973684 56.30947 0 77.793784l344.188016 353.383446-344.188016 353.384469c-20.973684 21.484314-20.973684 56.311517 0 77.79276 20.971637 21.482267 54.975079 21.482267 75.948763 0l382.072858-392.280337 0.001024-0.004094zM440.60802 154.908092l344.18597 353.383446-344.18597 353.385493c-20.973684 21.484314-20.973684 56.311517 0 77.79276 20.972661 21.482267 54.975079 21.482267 75.949786 0l382.074905-392.281361c20.966521-21.478174 20.966521-56.307424 0-77.790714L516.555759 77.115332c-20.972661-21.47715-54.975079-21.47715-75.949786 0-20.971637 21.48329-20.971637 56.30947 0.002047 77.79276z"
                p-id="2694">
          </path>
        </svg>
      </button>


      <div ref="VerifyImgHostRef"
           class="verify-img-host"
           :class="{'d-none': !state.IsShowVerifyImg }"
           :style="{'background-image':`url(${state.VerifyImgUrl})`}">
        <button :style="{
                   width:  state.BtnImgSliderWidth+'px',
                   height: state.BtnImgSliderHeight+'px',
                   left: state.BtnImgSliderPositionX+'px',
                   top: state.BtnImgSliderPositionY+'px',
                   'background-image':`url(${state.ImgSliderUrl})`
                   }"
                ref="BtnImgSliderRef"
                class="btn-imgslider"
                @mousedown="HandleBtnImgSliderMousedown">
        </button>
      </div>
    </div>
  </div>

</template>

<script setup lang="ts">
  import { ref, onMounted, onUnmounted, watch, reactive, nextTick } from 'vue'
  import type { ComponentPublicInstance } from 'vue'
  import type { IInputEvents } from '../Interfaces/IInputEvents';
  import { GenericOperateResult, SimpleOperateResult } from '../Commons/OperateResult/OperateResult'
  import { AxiosUtil } from '../Commons/Network/AxiosUtil';
  import type { ILoadingEvents } from '../Interfaces/ILoadingEvents';
  import type { IMessageEvents } from '../Interfaces/IMessageEvents';
  import type { IImgVerifyEvents } from '../Interfaces/IImgVerifyEvents';
  import { ImgVerifyModel } from '../Models/WebAPI/ImgVerifyModel'
  import { ImgVerifyResultModel } from '../Models/ImgVerifyResultModel';
  import type { IPoint } from '../Interfaces/IPoint';
  import { RectModel } from '../Models/WebAPI/RectModel';
  const axiosUtil = new AxiosUtil();

  interface MouseTrackPoint {
    x: number;
    y: number;
  }

  const props = defineProps<{
    LoadingEvents: ILoadingEvents | null;
    MessageEvents: IMessageEvents | null;
    OnBtnSliderMousedown?: () => Promise<SimpleOperateResult>;
  }>();

  // 创建输入框的引用
  const SliderBorderRef = ref<HTMLElement>();
  const BtnSliderRef = ref<HTMLElement>();
  const VerifyImgHostRef = ref<HTMLElement>();
  const BtnImgSliderRef = ref<HTMLElement>();
  // 监听组件显示状态变化
  const ElementRef = ref<HTMLElement>();
  const resizeObserver = ref<ResizeObserver>();

  const state = reactive({
    //这是滑块的位置X
    BtnSliderPositionX: 0,
    //这是滑动背景色百分比
    BackgroundFillPercent: 0,
    //图片滑动块宽度
    BtnImgSliderWidth: 0,
    //图片滑动块高度
    BtnImgSliderHeight: 0,
    //是否显示验证图像
    IsShowVerifyImg: false,
    //图片滑动块位置Left
    BtnImgSliderPositionX: 0,
    //图片滑动块位置Top
    BtnImgSliderPositionY: 0,
    //验证滑动块图像
    ImgSliderUrl: null as string | null,
    //验证背景
    VerifyImgUrl: null as string | null,
    Verify: null as RectModel | null,
    VerifySessionId: null as string | null,
  })

  let MouseMovingTrack: MouseTrackPoint[] = [];
  let IsBtnSliderDragging = false;
  let BtnSliderStartX = 0;
  let BtnSliderHistoryPositionX = 0;
  let BtnSliderWidth = 0;
  let BtnSliderHeight = 0;
  let BtnSliderContainerWidth = 0;
  let BtnSliderContainerHeight = 0;
  let BtnSliderMaxPositionX = 0;

  //这是图像拖动按钮的变量
  let IsBtnImgSliderDragging = false;
  let BtnImgSliderStartX = 0;
  let BtnImgSliderStartY = 0;
  let BtnImgSliderHistoryPositionX = 0;
  let BtnImgSliderHistoryPositionY = 0;

  let BtnImgSliderMaxPositionX = 0;
  let BtnImgSliderMaxPositionY = 0;
  //缩放比
  let WidthScale = 0;
  let HeightScale = 0;
  let MAX_POINTS = 1024;

  function InitBtnSliderControl() {

    if (BtnSliderRef.value == null) {
      return;
    }

    if (SliderBorderRef.value == null) {
      return;
    }

    //获取滑块的长度和宽度
    BtnSliderWidth = BtnSliderRef.value.clientWidth
    BtnSliderHeight = BtnSliderRef.value.clientHeight;
    BtnSliderContainerWidth = SliderBorderRef.value.clientWidth;
    BtnSliderContainerHeight = SliderBorderRef.value.clientHeight;
    //获取最大移动距离
    BtnSliderMaxPositionX = BtnSliderContainerWidth - BtnSliderWidth;


  }

  async function InitVerifyControlAsync(): Promise<SimpleOperateResult> {
    //计算按钮的大小和宽度
    if (VerifyImgHostRef.value == null) {
      return SimpleOperateResult.CreateFailResult("无法获取图像容器");
    }

    if (BtnImgSliderRef.value == null) {
      return SimpleOperateResult.CreateFailResult("无法获取验证控件");
    }

    const getImgVerifyModelResult = await axiosUtil.DecryptGet<ImgVerifyModel>('/api/v1/Security/GetImgVerifyModel', ImgVerifyModel);
    if (!getImgVerifyModelResult.IsSuccess) {
      return getImgVerifyModelResult;
    }
    let imgVerifyModel = getImgVerifyModelResult.Data;
    if (imgVerifyModel == null) {
      return SimpleOperateResult.CreateFailResult("未获取到验证码信息");
    }
    imgVerifyModel = ImgVerifyModel.GetBlobUrl(imgVerifyModel);

    state.VerifyImgUrl = imgVerifyModel.VerifyImgUrl;
    state.ImgSliderUrl = imgVerifyModel.ImgSliderUrl;
    state.VerifySessionId = imgVerifyModel.VerifySessionId;
    WidthScale = VerifyImgHostRef.value.clientWidth / imgVerifyModel.ImgWidth;
    HeightScale = VerifyImgHostRef.value.clientHeight / imgVerifyModel.ImgHeight;
    state.BtnImgSliderWidth = imgVerifyModel.IconWidth * WidthScale;
    state.BtnImgSliderHeight = imgVerifyModel.IconHeight * HeightScale;


    //获取最大移动距离
    BtnImgSliderMaxPositionX = VerifyImgHostRef.value.clientWidth - state.BtnImgSliderWidth;
    BtnImgSliderMaxPositionY = VerifyImgHostRef.value.clientHeight - state.BtnImgSliderHeight;

    //设置默认位置
    state.BtnImgSliderPositionX = imgVerifyModel.ImgBtnPositionX * WidthScale;
    state.BtnImgSliderPositionY = imgVerifyModel.ImgBtnPositionY * HeightScale;

    BtnImgSliderHistoryPositionX = state.BtnImgSliderPositionX;
    BtnImgSliderHistoryPositionY = state.BtnImgSliderPositionY;


    return SimpleOperateResult.CreateSuccessResult();
  }

  function HandleGlobalMouseMove(event: MouseEvent) {
    if (IsBtnSliderDragging) {
      HandleBtnSliderMove(event);
    } else if (IsBtnImgSliderDragging) {
      HandleBtnImgSliderMove(event);
    }
  }

  //滑动块事件
  async function HandleBtnSliderMove(event: MouseEvent): Promise<void> {
    if (props.LoadingEvents == null) {
      return;
    }

    let newPositionX = BtnSliderHistoryPositionX + event.clientX - BtnSliderStartX;
    newPositionX = Math.min(newPositionX, BtnSliderMaxPositionX);
    newPositionX = Math.max(0, newPositionX);
    state.BtnSliderPositionX = newPositionX;
    state.BackgroundFillPercent = Math.floor((state.BtnSliderPositionX + BtnSliderWidth) / BtnSliderContainerWidth * 100);

    if (state.BackgroundFillPercent > 99 && !state.IsShowVerifyImg) {
      state.IsShowVerifyImg = true;
      //在这里发起获取验证码请求
      const getRegisterVerifyResult = await props.LoadingEvents.SimpleLoadingActionAsync(async () => {
        return await InitVerifyControlAsync();
      });
      //检查结果
      if (!getRegisterVerifyResult.IsSuccess) {
        props.MessageEvents?.ShowDangerMsg(getRegisterVerifyResult.Message);
        return;
      }
    } else if (state.IsShowVerifyImg && state.BackgroundFillPercent < 20) {
      state.IsShowVerifyImg = false;
    }
  }

  //处理验证码拖动按钮事件
  function HandleBtnImgSliderMove(event: MouseEvent) {
    let newPositionX = BtnImgSliderHistoryPositionX + event.clientX - BtnImgSliderStartX;
    newPositionX = Math.min(newPositionX, BtnImgSliderMaxPositionX);
    newPositionX = Math.max(0, newPositionX);
    state.BtnImgSliderPositionX = newPositionX;

    let newPositionY = BtnImgSliderHistoryPositionY + event.clientY - BtnImgSliderStartY;
    newPositionY = Math.min(newPositionY, BtnImgSliderMaxPositionY);
    newPositionY = Math.max(0, newPositionY);
    state.BtnImgSliderPositionY = newPositionY;


    if (MouseMovingTrack.length > MAX_POINTS) {
      MouseMovingTrack.shift();
    }

    MouseMovingTrack.push({
      x: event.clientX,
      y: event.clientY
    });
  }

  //处理滑动按钮鼠标按下事件
  async function HandleBtnSliderMousedown(event: MouseEvent): Promise<void> {
    if (BtnSliderRef.value == null) {
      return;
    }
    if (SliderBorderRef.value == null) {
      return;
    }

    if (props.OnBtnSliderMousedown) {
      let operateResult = await props.OnBtnSliderMousedown();
      if (!operateResult.IsSuccess) {
        return;
      }
    }

    IsBtnSliderDragging = true;

    BtnSliderStartX = event.clientX;
  }

  //图像拖动快鼠标按下事件
  function HandleBtnImgSliderMousedown(event: MouseEvent) {

    if (VerifyImgHostRef.value == null) {
      return;
    }
    if (BtnImgSliderRef.value == null) {
      return;
    }
    IsBtnImgSliderDragging = true;
    MouseMovingTrack = [];
    BtnImgSliderStartX = event.clientX;
    BtnImgSliderStartY = event.clientY;
  }

  //窗体鼠标弹起事件
  function HandleGlobalMouseUp() {

    IsBtnSliderDragging = false;
    IsBtnImgSliderDragging = false;
    BtnSliderHistoryPositionX = state.BtnSliderPositionX;
    BtnImgSliderHistoryPositionX = state.BtnImgSliderPositionX;
    BtnImgSliderHistoryPositionY = state.BtnImgSliderPositionY;

    state.Verify = new RectModel();
    state.Verify.Left = state.BtnImgSliderPositionX;
    state.Verify.Top = state.BtnImgSliderPositionY;
    state.Verify.Width = state.BtnImgSliderWidth;
    state.Verify.Height = state.BtnImgSliderHeight;
  }

  //处理鼠标离开
  function HandleGlobalMouseleave() {
    HandleGlobalMouseUp();
  }

  // 使用箭头函数确保this指向正确
  const handleGlobalMouseMove = (event: MouseEvent) => {
    HandleGlobalMouseMove(event);
  };

  const handleGlobalMouseUp = () => {
    HandleGlobalMouseUp();
  };

  const handleGlobalMouseleave = () => {
    HandleGlobalMouseleave();
  };

  // 添加全局事件监听
  window.addEventListener('mousemove', handleGlobalMouseMove);
  window.addEventListener('mouseup', handleGlobalMouseUp);
  window.addEventListener('mouseleave', handleGlobalMouseleave);



  // 初始化ResizeObserver
  const initResizeObserver = () => {
    if (resizeObserver.value) {
      resizeObserver.value.disconnect();
    }

    resizeObserver.value = new ResizeObserver(() => {
      if (ElementRef.value != null && ElementRef.value?.offsetWidth > 0) {
        InitBtnSliderControl();
      }
    });

    if (ElementRef.value) {
      resizeObserver.value.observe(ElementRef.value);
    }
  };

  onMounted(() => {
    initResizeObserver();
  });

  onUnmounted(() => {
    if (resizeObserver.value) {
      resizeObserver.value.disconnect();
    }
    window.removeEventListener('mousemove', handleGlobalMouseMove);
    window.removeEventListener('mouseup', handleGlobalMouseUp);
    window.removeEventListener('mouseleave', handleGlobalMouseleave);
  });

  function GetImgVerifyResultAsync(): Promise<GenericOperateResult<ImgVerifyResultModel>> {
    return new Promise((resolve) => {
      if (state.IsShowVerifyImg) {
        const imgVerifyResultModel = new ImgVerifyResultModel();
        const verify = new RectModel();

        verify.Left = state.Verify == null ? 0 : state.Verify.Left / WidthScale;
        verify.Top = state.Verify == null ? 0 : state.Verify.Top / HeightScale;
        verify.Width = state.Verify == null ? 0 : state.Verify.Width / WidthScale;
        verify.Height = state.Verify == null ? 0 : state.Verify.Height / HeightScale;

        imgVerifyResultModel.Verify = verify;
        imgVerifyResultModel.VerifySessionId = state.VerifySessionId;
        resolve(GenericOperateResult.CreateSuccessResult<ImgVerifyResultModel>(imgVerifyResultModel));
      } else {
        resolve(GenericOperateResult.CreateFailResult<ImgVerifyResultModel>("获取验证码失败"));
      }
    });
  }

  // 导出方法供父组件调用
  defineExpose<IImgVerifyEvents>({
    GetImgVerifyResultAsync
  });

  //// 定义要触发的事件
  //const emits = defineEmits<{
  //  (e: 'OnBtnSliderMousedown');
  //}>();

</script>


