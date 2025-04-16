<style scoped>
  .email-verify-header {
    font-weight: 600;
    font-size: 20px;
    color: #303133;
    justify-content: center;
    display: flex;
    text-align: center;
    width: 100%;
    padding: 0px 0px 0px 5px;
    margin-bottom: 24px;
    letter-spacing: 1px;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  }

  .verify-input-border {
    display: flex;
    width: 100%;
    margin: 50px 0px 24px 0px;
    padding: 0px 1px 0px 1px;
  }

  .verify-input {
    aspect-ratio: 1;
    width: 100%;
    border: 1px solid #dcdfe6;
    border-radius: 6px;
    text-align: center;
    font-size: 20px;
    font-weight: 500;
    color: #303133;
    transition: all 0.3s;
    outline: none;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    margin: 5px;
  }

    .verify-input:focus {
      border-color: #409eff;
      box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.2);
    }

  .btn-email-verify {
    width: 100%;
    height: 44px;
    background-color: #409eff;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 500;
    letter-spacing: 1px;
    transition: all 0.3s;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  }

    .btn-email-verify:hover {
      background-color: #66b1ff;
    }

    .btn-email-verify:active {
      background-color: #3a8ee6;
    }

  /* .email-verify-right {
    width: 400px;
    padding: 40px;
    background-color: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
  }*/

  .form-row {
    width: 100%;
  }
</style>
<template>
  <div class="div-main">
    <RSMessage :ref="events => ViewModel.RSMessageEvents= events"></RSMessage>

    <div class="email-verify-content">
      <div class="email-verify-left img">
      </div>
      <RSLoading LoadingType="Rotate" :ref="events => ViewModel.RSLoadingEvents= events">
        <div class="email-verify-right">
          <div class="email-verify-header">邮箱验证</div>
          <div class="verify-input-border">
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.VerifyList.value[0]" @input="(e) => ViewModel.HandleInput(0, e)" @keydown="(e) => ViewModel.HandleKeyDown(0, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref=> ViewModel.RSInputList[0] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.VerifyList.value[1]" @input="(e) => ViewModel.HandleInput(1, e)" @keydown="(e) => ViewModel.HandleKeyDown(1, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.RSInputList[1] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.VerifyList.value[2]" @input="(e) => ViewModel.HandleInput(2, e)" @keydown="(e) => ViewModel.HandleKeyDown(2, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.RSInputList[2] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.VerifyList.value[3]" @input="(e) => ViewModel.HandleInput(3, e)" @keydown="(e) => ViewModel.HandleKeyDown(3, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.RSInputList[3] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.VerifyList.value[4]" @input="(e) => ViewModel.HandleInput(4, e)" @keydown="(e) => ViewModel.HandleKeyDown(4, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.RSInputList[4] = ref" />
            <input class="verify-input" type="text" maxlength="1" v-model="ViewModel.VerifyList.value[5]" @input="(e) => ViewModel.HandleInput(5, e)" @keydown="(e) => ViewModel.HandleKeyDown(5, e)" @paste="(e) => ViewModel.HandlePaste(e)" :ref="ref => ViewModel.RSInputList[5] = ref" />
          </div>
          <div class="form-row">
            <button type="button" class="btn-email-verify" @click="ViewModel.HandleVerfiyConfirm()">确认</button>
          </div>
        </div>
      </RSLoading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import RSMessage from '../../Controls/RSMessage.vue'
  import RSLoading from '../../Controls/RSLoading.vue'
  import { EmailVerifyViewModel } from './EmailVerifyViewModel'
  const ViewModel = new EmailVerifyViewModel()
</script>
