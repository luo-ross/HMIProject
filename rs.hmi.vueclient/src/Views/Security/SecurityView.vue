

<template>
  <div class="div-main">
    <RSMessage :ref="events => ViewModel.RSMessageEvents= events"></RSMessage>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <RSLoading LoadingType="Rotate" :ref="events => ViewModel.RSLoadingEvents= events">
        <div v-if="!ViewModel.SecurityModel.IsEmailSendSuccucess" class="general-content-right">
          <h2 class="text-2xl font-bold text-gray-700 mb-4 mt-4 text-center">通过邮箱找回密码</h2>
          <RSEmail Placeholder="请输入邮箱"
                   class="security-email"
                   v-model:Email="ViewModel.SecurityModel.Email"
                   :ref="events => ViewModel.RSEmailEvents= events" />

          <div class="form-row">
            <button type="button" class="btn-custorm-primary" @click="ViewModel.HandlePasswordReset">发送重置邮件</button>
          </div>
        </div>

        <div v-else class="general-content-right">
          <IconSuccess class="icon-success"></IconSuccess>
          <div class="card-title fw-bold mb-3 mt-5">邮件发送成功</div>
          <div class="card-text text-muted ">请查看邮箱收件箱或者垃圾邮件</div>
          <div class="card-text text-muted mb-4">然后点击右键里的密码重置连接进行密码修改</div>
          <button class="btn-custorm-primary" @click="ViewModel.HandleReturnSecurity">返回</button>
        </div>
      </RSLoading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import RSMessage from '../../Controls/RSMessage.vue'
  import RSEmail from '../../Controls/RSEmail.vue'
  import RSLoading from '../../Controls/RSLoading.vue'
  import { SecurityViewModel } from './SecurityViewModel'
  import IconSuccess from '../../Controls/Icons/IconSuccess.vue'
  const ViewModel = new SecurityViewModel()
</script>
