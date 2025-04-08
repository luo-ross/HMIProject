<template>
  <div class="div-main">
    <Message v-model:Message="ViewModel.Message.value"
             v-model:MessageType="ViewModel.MessageType.value"
             ></Message>

    <div class="register-content">
      <div class="register-left img">
      </div>
      <div class="register-right">
        <div class="tab-border">
          <input type="radio" id="radio-login" name="tab" value="radio-login">
          <label class="label-tab label-login" for="radio-login" @click="HandleLogin">登录</label>

          <input type="radio" checked id="radio-register" name="tab" value="radio-register">
          <label class="label-tab label-register" for="radio-register">注册</label>
        </div>

        <InputEmail Placeholder="请输入邮箱"
                    v-model:Email="ViewModel.Email.value"
                    ref="EmailInputRef" />

        <InputPassword 
                       Placeholder="请输入密码"
                       v-model:Password="ViewModel.Password.value"
                       ref="PasswordInputRef" />

        <InputPassword 
                       Placeholder="请确认密码"
                       v-model:Password="ViewModel.PasswordConfirm.value"
                       ref="PasswordConfirmInputRef" />

        <div class="form-row">
          <button type="button" class="btn-register-next" @click="ViewModel.HandleRegisterNext">下一步</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import Message from '../../components/Message.vue'
  import InputEmail from '../../components/InputEmail.vue'
  import InputPassword from '../../components/InputPassword.vue'
  import Verify from '../../components/Verify.vue'
  import { useRouter } from 'vue-router'
  import { RegisterViewModel } from './RegisterViewModel'
  import { ref, onMounted } from 'vue'
  
  const Router = useRouter()
  const ViewModel = new RegisterViewModel()
  const EmailInputRef = ref()
  const PasswordInputRef = ref()
  const PasswordConfirmInputRef = ref()

  onMounted(() => {
    ViewModel.SetEmailInputRef(EmailInputRef.value)
    ViewModel.SetPasswordInputRef(PasswordInputRef.value)
    ViewModel.SetPasswordConfirmInputRef(PasswordConfirmInputRef.value)
  })

  const HandleLogin = () => {
    Router.push('/login/index')
  }
</script> 
