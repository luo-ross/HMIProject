<style scoped>
  .error-message {
    width: 100%;
    min-height: 30px;
    justify-content: center;
    display: flex;
    align-items: center;
    padding: 5px 15px;
    position: absolute;
    top: 0px;
    z-index: 11111;
  }
</style>

<template>
  <div class="error-message alert-danger" v-if="MessageType === 'danger'">
    {{ Message }}
  </div>
  <div class="error-message alert-dark" v-else-if="MessageType === 'dark'">
    {{ Message }}
  </div>
  <div class="error-message alert-dismissible" v-else-if="MessageType === 'dismissible'">
    {{ Message }}
  </div>
  <div class="error-message alert-heading" v-else-if="MessageType === 'heading'">
    {{ Message }}
  </div>
  <div class="error-message alert-info" v-else-if="MessageType === 'info'">
    {{ Message }}
  </div>
  <div class="error-message alert-light" v-else-if="MessageType === 'light'">
    {{ Message }}
  </div>
  <div class="error-message alert-link" v-else-if="MessageType === 'link'">
    {{Message }}
  </div>
  <div class="error-message alert-primary" v-else-if="MessageType === 'primary'">
    {{ Message }}
  </div>
  <div class="error-message alert-secondary" v-else-if="MessageType === 'secondary'">
    {{ Message }}
  </div>
  <div class="error-message alert-success" v-else-if="MessageType === 'success'">
    {{Message }}
  </div>
  <div class="error-message alert-warning" v-else-if="MessageType === 'warning'">
    {{ Message }}
  </div>
  <div class="error-message d-none" v-else>

  </div>
</template>

<script setup lang="ts">
  import { ref, computed } from 'vue'
  import IMessageEvents from '../Interfaces/IMessageEvents'
  import { MessageEnum } from '../Commons/Enums/MessageEnum'

  const Message = defineModel('Message');
  const MessageType = defineModel('MessageType');

  let TimerId: number = -1;
  function ShowDangerMsg(msg: string): void {
    ShowMsg(MessageEnum.Danger, msg);
  }

  function ShowDarkMsg(msg: string): void {
    ShowMsg(MessageEnum.Dark, msg);
  }

  function ShowDismissibleMsg(msg: string): void {
    ShowMsg(MessageEnum.Dismissible, msg);
  }

  function ShowHeadingMsg(msg: string): void {
    ShowMsg(MessageEnum.Heading, msg);
  }


  function ShowInfoMsg(msg: string): void {
    ShowMsg(MessageEnum.Info, msg);
  }


  function ShowLightMsg(msg: string): void {
    ShowMsg(MessageEnum.Light, msg);
  }


  function ShowLinkMsg(msg: string): void {
    ShowMsg(MessageEnum.Link, msg);
  }


  function ShowPrimaryMsg(msg: string): void {
    ShowMsg(MessageEnum.Primary, msg);
  }


  function ShowSecondaryMsg(msg: string): void {
    ShowMsg(MessageEnum.Secondary, msg);
  }


  function ShowSuccessMsg(msg: string): void {
    ShowMsg(MessageEnum.Success, msg);
  }

  /**
   * 显示信息消息
   */
  function ShowWarningMsg(msg: string): void {
    ShowMsg(MessageEnum.Warning, msg);
  }

  /**
   * 清除消息
   */
  function ClearMsg(): void {
    Message.value = '';
    MessageType.value = '';
  }

  /**
   * 显示消息
   */
  function ShowMsg(type: MessageType, msg: string): void {
    ClearMsg();
    Message.value = msg;
    MessageType.value = type;
    TimerId = setTimeout(() => {
      ClearMsg();
      TimerId = -1;
    }, 3000);
  }


  // 暴露方法给父组件
  defineExpose<IMessageEvents>({
    ShowDangerMsg,
    ShowDarkMsg,
    ShowDismissibleMsg,
    ShowHeadingMsg,
    ShowInfoMsg,
    ShowLightMsg,
    ShowLinkMsg,
    ShowPrimaryMsg,
    ShowSecondaryMsg,
    ShowSuccessMsg,
    ShowWarningMsg,
    ShowMsg,
    ClearMsg,
  });
</script>
