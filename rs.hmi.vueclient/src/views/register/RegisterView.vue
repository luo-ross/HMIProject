<template>
  <div class="div-main">
    <RSMessage v-model:Message="ViewModel.MessageModel.Message"
               v-model:MessageType="ViewModel.MessageModel.MessageType"></RSMessage>

    <div class="register-content">
      <div class="register-left img">
      </div>
      <RSLoading>
        <div class="register-right">
          <div class="tab-border">
            <input type="radio" id="radio-login" name="tab" value="radio-login">
            <label class="label-tab label-login" for="radio-login" @click="HandleLogin">登录</label>

            <input type="radio" checked id="radio-register" name="tab" value="radio-register">
            <label class="label-tab label-register" for="radio-register">注册</label>
          </div>

          <RSEmail Placeholder="请输入邮箱"
                   v-model:Email="ViewModel.RegisterModel.Email"
                   ref="ViewModel.RSEmailRef" />

          <RSPassword Placeholder="请输入密码"
                      v-model:Password="ViewModel.RegisterModel.Password"
                      ref="ViewModel.RSPasswordRef" />

          <RSPassword Placeholder="请确认密码"
                      v-model:Password="ViewModel.RegisterModel.PasswordConfirm"
                      ref="ViewModel.RSPasswordConfirmRef" />

          <div class="form-row">
            <button type="button" class="btn-register-next" @click="ViewModel.HandleRegisterNext">下一步</button>
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
  import { useRouter } from 'vue-router'
  import { RegisterViewModel } from './RegisterViewModel'
  import { ref, onMounted } from 'vue'
  const Router = useRouter()
  const ViewModel = new RegisterViewModel()
  //const RSEmailRef = ref()
  //const RSPasswordRef = ref()
  //const RSPasswordConfirmRef = ref()
  //onMounted(() => {
  //  ViewModel.SetRSEmailRef(RSEmailRef.value)
  //  ViewModel.SetRSPasswordRef(RSPasswordRef.value)
  //  ViewModel.SetRSPasswordConfirmRef(RSPasswordConfirmRef.value)
  //})

  const HandleLogin = () => {
    Router.push('/Login/Index')
  }
</script>
