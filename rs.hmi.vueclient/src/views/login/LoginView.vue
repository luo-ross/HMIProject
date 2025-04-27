<template>
  <div class="div-main">
    <Message :ref="events => ViewModel.MessageEvents= events"></Message>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <Loading LoadingType="ProgressBar" :ref="events => ViewModel.LoadingEvents= events">
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


          <Email Placeholder="请输入邮箱"
                 v-model:Email="ViewModel.LoginModel.Email"
                 :ref="events => ViewModel.EmailEvents= events" />


          <Password Placeholder="请输入密码"
                    v-model:Password="ViewModel.LoginModel.Password"
                    :ref="events => ViewModel.PasswordEvents= events" />

          <ImgVerify
                     :ref="events => ViewModel.ImgVerifyEvents= events"
                     v-model:LoadingEvents="ViewModel.LoadingEvents"
                     v-model:MessageEvents="ViewModel.MessageEvents">
          </ImgVerify>


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


      </Loading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, watch, computed } from 'vue'
  import Message from '../../Controls/Message.vue'
  import Email from '../../Controls/Email.vue'
  import Password from '../../Controls/Password.vue'
  import Loading from '../../Controls/Loading.vue'
  import ImgVerify from '../../Controls/ImgVerify.vue'
  import { LoginViewModel } from './LoginViewModel'
  const ViewModel = new LoginViewModel()

  //// 判断是否都已输入
  //const canShowCaptcha = computed(() => {
  //  return ViewModel.LoginModel.Email != null
  //    && ViewModel.LoginModel.Password != null
  //    && ViewModel.LoginModel.Email?.trim() !== ''
  //    && ViewModel.LoginModel.Password?.trim() !== '';
  //});
</script>



