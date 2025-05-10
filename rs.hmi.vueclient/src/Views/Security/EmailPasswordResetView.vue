

<template>
  <div class="div-main">
    <Message :ref="ViewModel.MessageEvents"></Message>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <Loading LoadingType="Rotate" :ref="ViewModel.LoadingEvents">
        <div v-if="!ViewModel.EmailPasswordResetModel.IsPasswordResetSuccucess" class="general-content-right">
          <h2 class="text-2xl font-bold text-gray-700 mb-4 mt-4 text-center">密码重置</h2>

          <Password class="mt-4" Placeholder="请输入密码"
                      v-model:Password="ViewModel.EmailPasswordResetModel.Password"
                      :ref="ViewModel.PasswordEvents" />

          <Password Placeholder="请确认密码"
                      v-model:Password="ViewModel.EmailPasswordResetModel.PasswordConfirm"
                      :ref="ViewModel.PasswordConfirmEvents" />

          <div class="form-row">
            <button type="button" class="btn-custorm-primary  mt-5" @click="ViewModel.HandleEmailPasswordReset">确认</button>
          </div>
          <div class="form-row">
            <a class="btn-redirect" @click="ViewModel.HandleBackSecurity">返回密码重置?</a>
          </div>
        </div>

        <div v-else class="general-content-right">
          <IconSuccess class="icon-success"></IconSuccess>
          <h2 class="text-2xl font-bold text-gray-700 mb-4 mt-4 text-center">密码重置成功</h2>
          <div class="card-text text-muted mb-4">请点击下面按钮跳转到登录界面进行登录</div>
          <button class="btn-custorm-primary mt-5" @click="ViewModel.HandleRedirectToLoginView">跳转到登录页面</button>
        </div>
      </Loading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import Message from '../../Controls/Message.vue'
  import Loading from '../../Controls/Loading.vue'
  import Password from '../../Controls/Password.vue'
  import { EmailPasswordResetViewModel } from './EmailPasswordResetViewModel'
  import IconSuccess from '../../Controls/Icons/IconSuccess.vue'
  const ViewModel = new EmailPasswordResetViewModel()
</script>
