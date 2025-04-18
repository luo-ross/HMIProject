

<template>
  <div class="div-main">
    <RSMessage :ref="events => ViewModel.RSMessageEvents= events"></RSMessage>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <RSLoading LoadingType="Rotate" :ref="events => ViewModel.RSLoadingEvents= events">
        <div class="general-content-right">
          <h2 class="text-2xl font-bold text-gray-700 mb-4 mt-4 text-center">通过邮箱找回密码</h2>

          <RSEmail Placeholder="请输入邮箱"
                   class="security-email"
                   v-model:Email="ViewModel.EmailSecurityModel.Email"
                   :ref="events => ViewModel.RSEmailEvents= events" />

          <div class="form-row">
            <button type="button" class="btn-custorm-primary" @click="ViewModel.HandlePasswordReset">发送重置邮件</button>
          </div>
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
  const ViewModel = new SecurityViewModel()
</script>
