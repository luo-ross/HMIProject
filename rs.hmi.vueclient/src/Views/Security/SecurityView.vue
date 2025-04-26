

<template>
  <div class="div-main">
    <Message :ref="events => ViewModel.MessageEvents= events"></Message>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <Loading LoadingType="ProgressBar" :ref="events => ViewModel.LoadingEvents= events">
        <div v-if="!ViewModel.SecurityModel.IsEmailSendSuccucess" class="general-content-right">
          <h2 class="text-2xl font-bold text-gray-700 mb-4 mt-4 text-center">通过邮箱找回密码</h2>
          <Email Placeholder="请输入邮箱"
                   class="security-email"
                   v-model:Email="ViewModel.SecurityModel.Email"
                   :ref="events => ViewModel.EmailEvents= events" />

          <div class="form-row">
            <button type="button" class="btn-custorm-primary mt-4" @click="ViewModel.HandSendEmailPasswordReset">发送重置邮件</button>
          </div>
          <div class="form-row">
            <a class="btn-redirect" @click="ViewModel.HandleBackLogin">返回登录?</a>
          </div>
        </div>

        <div v-else class="general-content-right">
          <IconSuccess class="icon-success"></IconSuccess>
          <h2 class="text-2xl font-bold text-gray-700 mb-4 mt-5 text-center">邮件发送成功</h2>
          <div class="card-text text-muted ">请查看邮箱收件箱或者垃圾邮件</div>
          <div class="card-text text-muted mb-4">然后点击右键里的密码重置连接进行密码修改</div>
          <button class="btn-custorm-primary" @click="ViewModel.HandleReturnSecurity">返回</button>
        </div>
      </Loading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import Message from '../../Controls/Message.vue'
  import Email from '../../Controls/Email.vue'
  import Loading from '../../Controls/Loading.vue'
  import { SecurityViewModel } from './SecurityViewModel'
  import IconSuccess from '../../Controls/Icons/IconSuccess.vue'
  const ViewModel = new SecurityViewModel()
</script>
