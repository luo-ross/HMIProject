
<template>
  <div class="div-main">
    <RSMessage :ref="events => ViewModel.RSMessageEvents= events"></RSMessage>
    <div class="general-content">
      <div class="general-content-left img">
      </div>
      <RSLoading LoadingType="Rotate" :ref="events => ViewModel.RSLoadingEvents= events">
        <div v-if="!ViewModel.EmailVerifyModel.IsRegisterSuccucess" class="general-content-right">
          <div class="email-verify-header">邮箱验证</div>
          <div class="email-info">请查看邮箱获取验证码</div>
          <div class="email-info">{{ ViewModel.EmailVerifyModel.Email }}</div>
          <div class="verify-input-border">
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.EmailVerifyModel.VerifyList[0]" @input="(e) => ViewModel.HandleInput(0, e)" @keydown="(e) => ViewModel.HandleKeyDown(0, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref=>  ViewModel.EmailVerifyModel.RSInputList[0] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.EmailVerifyModel.VerifyList[1]" @input="(e) => ViewModel.HandleInput(1, e)" @keydown="(e) => ViewModel.HandleKeyDown(1, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.EmailVerifyModel.RSInputList[1] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.EmailVerifyModel.VerifyList[2]" @input="(e) => ViewModel.HandleInput(2, e)" @keydown="(e) => ViewModel.HandleKeyDown(2, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.EmailVerifyModel.RSInputList[2] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.EmailVerifyModel.VerifyList[3]" @input="(e) => ViewModel.HandleInput(3, e)" @keydown="(e) => ViewModel.HandleKeyDown(3, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.EmailVerifyModel.RSInputList[3] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.EmailVerifyModel.VerifyList[4]" @input="(e) => ViewModel.HandleInput(4, e)" @keydown="(e) => ViewModel.HandleKeyDown(4, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.EmailVerifyModel.RSInputList[4] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.EmailVerifyModel.VerifyList[5]" @input="(e) => ViewModel.HandleInput(5, e)" @keydown="(e) => ViewModel.HandleKeyDown(5, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.EmailVerifyModel.RSInputList[5] = ref" />
          </div>
          <div class="form-row">
            <button type="button" class="btn-custorm-primary" @click="ViewModel.HandleVerfiyConfirm()">确认</button>
          </div>
          <div class="countdown">剩余时间：{{ ViewModel.EmailVerifyModel.RemainingSeconds }}秒</div>
        </div>
        <div v-else class="general-content-right">
          <IconSuccess class="icon-success"></IconSuccess>
          <div class="card-title fw-bold mb-3 mt-5">注册成功</div>
          <div class="card-text text-muted mb-4">你已成功注册账号。欢迎加入我们，开启全新的旅程</div>
          <button class="btn-custorm-primary" @click="ViewModel.HandleBackLogin">返回登录</button>
        </div>
      </RSLoading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import RSMessage from '../../Controls/RSMessage.vue'
  import RSLoading from '../../Controls/RSLoading.vue'
  import { EmailVerifyViewModel } from './EmailVerifyViewModel'
  import IconSuccess from '../../Controls/Icons/IconSuccess.vue'
  const ViewModel = new EmailVerifyViewModel()
</script>
