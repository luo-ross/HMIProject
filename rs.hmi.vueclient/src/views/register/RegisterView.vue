
<template>

  <div class="div-main">
    <RSMessage :ref="events => ViewModel.RSMessageEvents= events"></RSMessage>
   
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <RSLoading LoadingType="Rotate" :ref="events => ViewModel.RSLoadingEvents= events">
        <div class="general-content-right">
          <div class="tab-border">
            <input type="radio" id="radio-login" name="tab" value="radio-login">
            <label class="label-tab label-login" for="radio-login" @click="ViewModel.HandleLogin">登录</label>

            <input type="radio" checked id="radio-register" name="tab" value="radio-register">
            <label class="label-tab label-register" for="radio-register">注册</label>
          </div>

          <RSEmail Placeholder="请输入邮箱"
                   v-model:Email="ViewModel.RegisterModel.Email"
                   :ref="events => ViewModel.RSEmailEvents= events" />

          <RSPassword Placeholder="请输入密码"
                      v-model:Password="ViewModel.RegisterModel.Password"
                      :ref="events => ViewModel.RSPasswordEvents= events" />

          <RSPassword Placeholder="请确认密码"
                      v-model:Password="ViewModel.RegisterModel.PasswordConfirm"
                      :ref="events => ViewModel.RSPasswordConfirmEvents= events" />

          <div class="form-row">
            <button type="button" class="btn-custorm-primary" @click="ViewModel.RegisterNextCommand.Execute()">下一步</button>
          </div>
        </div>
      </RSLoading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import RSMessage from '../../Controls/RSMessage.vue'
  import RSEmail from '../../Controls/RSEmail.vue'
  import RSPassword from '../../Controls/RSPassword.vue'
  import RSLoading from '../../Controls/RSLoading.vue'
  import { RegisterViewModel } from './RegisterViewModel'
  const ViewModel = new RegisterViewModel()
</script>
