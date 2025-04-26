
<template>

  <div class="div-main">
    <Message :ref="events => ViewModel.MessageEvents= events"></Message>
   
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <Loading LoadingType="Rotate" :ref="events => ViewModel.LoadingEvents= events">
        <div class="general-content-right">
          <div class="tab-border">
            <input type="radio" id="radio-login" name="tab" value="radio-login">
            <label class="label-tab label-login" for="radio-login" @click="ViewModel.HandleLogin">登录</label>

            <input type="radio" checked id="radio-register" name="tab" value="radio-register">
            <label class="label-tab label-register" for="radio-register">注册</label>
          </div>

          <Email Placeholder="请输入邮箱"
                   v-model:Email="ViewModel.RegisterModel.Email"
                   :ref="events => ViewModel.EmailEvents= events" />

          <Password Placeholder="请输入密码"
                      v-model:Password="ViewModel.RegisterModel.Password"
                      :ref="events => ViewModel.PasswordEvents= events" />

          <Password Placeholder="请确认密码"
                      v-model:Password="ViewModel.RegisterModel.PasswordConfirm"
                      :ref="events => ViewModel.PasswordConfirmEvents= events" />

          <div class="form-row">
            <button type="button" class="btn-custorm-primary" @click="ViewModel.RegisterNextCommand.Execute()">下一步</button>
          </div>
        </div>
      </Loading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import Message from '../../Controls/Message.vue'
  import Email from '../../Controls/Email.vue'
  import Password from '../../Controls/Password.vue'
  import Loading from '../../Controls/Loading.vue'
  import { RegisterViewModel } from './RegisterViewModel'
  const ViewModel = new RegisterViewModel()
</script>
