<style>
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

    .btn-slider:active ,.btn-imgslider:active{
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

  .btn-slider:hover,.btn-imgslider:hover {
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
</style>
<template>
  <div class="div-main">
    <RSMessage :ref="events => ViewModel.RSMessageEvents= events"></RSMessage>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <RSLoading LoadingType="ProgressBar" :ref="events => ViewModel.RSLoadingEvents= events">
        <div class="general-content-right">
          <a class="btn-qrcode-login" href="/Login/QRCode?ClientId=@clientId&Timestamp=@timeStamp">
            <svg t="1743521881584" class="icon-qrcode" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="9539" width="64" height="64">
              <defs>
                <linearGradient id="myGradient" x1="0%" y1="0%" x2="100%" y2="100%">
                  <stop offset="0%" stop-color="#198cff" />
                  <stop offset="100%" stop-color="#1afa29" />
                </linearGradient>
              </defs>
              <path d="M453.416 544.83c17.89 0 26.918 11.902 26.918 26.85v125.996l-80.697 80.661V625.495H142.706v259.82l149.905-0.001-74.72 74.685L94.909 960c-14.805 0-29.61-14.706-29.905-29.45l-0.004-0.447V571.681c0-11.903 9.029-26.795 29.909-26.795h358.564zM265.162 703.17a52.943 52.943 0 0 1 48.96 19.528 52.895 52.895 0 0 1 7.81 52.111c-20.88 53.758-119.465 32.887-101.577-35.82 3.048-17.937 17.945-32.83 44.807-35.819z m368.032-158.284l-87.118 87.079v-63.218c0-20.718 14.602-23.8 26.502-23.86h0.36l60.256-0.001zM453.416 64c20.936 0 26.918 17.938 26.918 29.897v358.366a26.79 26.79 0 0 1-7.85 19.06 26.811 26.811 0 0 1-19.068 7.847H94.91C76.963 479.17 65 470.145 65 449.273V90.907C65 75.96 79.954 64 91.918 64z m479.666 0C954.018 64 960 81.938 960 93.897l-0.001 124.334-80.696 80.659V141.675H622.372v259.82l154.28-0.001-77.711 77.676H574.574c-17.765 0-29.668-8.846-29.905-29.274l-0.003-0.623V90.907C544.666 75.96 559.62 64 571.583 64z m-533.445 77.675H142.706v259.82h256.931v-259.82z m-140.4 77.619c35.777-5.98 65.685 17.938 65.685 50.768 0 41.855-41.815 65.716-77.649 47.778v0.057c-38.825-20.928-35.89-89.578 11.963-98.603z m479.665 0c35.778-5.98 65.686 17.938 65.686 50.768 0 41.855-41.815 65.716-77.65 47.778v0.057c-38.824-20.928-35.89-89.578 11.964-98.603z" fill="url(#myGradient)" p-id="9540"></path>
            </svg>
          </a>

          <div class="tab-border">
            <input type="radio" checked id="radio-login" name="tab" value="radio-login">
            <label class="label-tab label-login" for="radio-login">登录</label>

            <input type="radio" id="radio-register" name="tab" value="radio-register">
            <label class="label-tab label-register" for="radio-register" @click="ViewModel.HandleRegister">注册</label>
          </div>


          <RSEmail Placeholder="请输入邮箱"
                   v-model:Email="ViewModel.LoginModel.Email"
                   :ref="events => ViewModel.RSEmailEvents= events" />


          <RSPassword Placeholder="请输入密码"
                      v-model:Password="ViewModel.LoginModel.Password"
                      :ref="events => ViewModel.RSPasswordEvents= events" />


          <!--<div class="form-row">
            <RSVerify Placeholder="请输入验证码"
                      v-model:Verify="ViewModel.LoginModel.Verify"
                      :ref="events => ViewModel.RSVerifyEvents = events" />
            <div class="form-input-border img-border">
              <img class="img-verify" src="/public/verify.jpg" />
            </div>
          </div>-->
          <div class="form-row">
            <div :ref="el => ViewModel.SliderBorderRef.value = el"
                 class="slider-border">
              <span class="slider-text">向右滑动滑块进行验证</span>
              <div :style="{width: ViewModel.LoginModel.BackgroundFillPercent+'%' }"
                   class="background-fill">
              </div>
              <button :ref="el => ViewModel.BtnSliderRef.value = el"
                      :style="{left: ViewModel.LoginModel.BtnSliderPositionX+'px' }"
                      class="btn-slider"
                      @mousedown="ViewModel.HandleBtnSliderMousedown($event)">
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
              <div :ref="el => ViewModel.VerifyImgHostRef.value = el"
                   class="verify-img-host"
                   :class="{'d-none': !ViewModel.LoginModel.IsShowVerifyImg }"
                   :style="{'background-image':`url(${ViewModel.LoginModel.VerifyImgUrl})`}"
                   >
               
                <button :style="{
                        width: ViewModel.LoginModel.BtnImgSliderWidth+'px',
                        height:ViewModel.LoginModel.BtnImgSliderHeight+'px',
                        left:ViewModel.LoginModel.BtnImgSliderPositionX+'px',
                        top:ViewModel.LoginModel.BtnImgSliderPositionY+'px',
                        'background-image':`url(${ViewModel.LoginModel.ImgSliderUrl})`
                        }"
                        :ref="el => ViewModel.BtnImgSliderRef.value = el"
                        class="btn-imgslider"
                        @mousedown="ViewModel.HandleBtnImgSliderMousedown($event)">
                </button>

                
              </div>
            </div>
          </div>


          <div class="form-row">
            <button type="button" class="btn-custorm-primary"
                    @click="ViewModel.HandleLogin">
              登录
            </button>
          </div>

          <div class="form-row">
            <a class="btn-redirect" @click="ViewModel.HandleForgetPassword">忘记了密码?</a>
          </div>

        </div>


      </RSLoading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue'
  import RSWindow from '../../Controls/RSWindow.vue'
  import RSMessage from '../../Controls/RSMessage.vue'
  import RSEmail from '../../Controls/RSEmail.vue'
  import RSPassword from '../../Controls/RSPassword.vue'
  import RSVerify from '../../Controls/RSVerify.vue'
  import RSLoading from '../../Controls/RSLoading.vue'
  import { LoginViewModel } from './LoginViewModel'
  const ViewModel = new LoginViewModel()
</script>



