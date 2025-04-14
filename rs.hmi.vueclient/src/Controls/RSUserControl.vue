<template>
  <div>
    <slot></slot>
    <RSModal x:Name="PART_Modal" />
    <RSLoading x:Name="PART_Loading" />
    <RSMessageBox x:Name="PART_MessageBox" />
    <RSMessage ref="RSMessageEvents"></RSMessage>
  </div>
</template>

<script setup lang="ts">
  import { ref } from 'vue';
  import Border from '../Controls/Border.vue'
  import RSMessage from '../Controls/RSMessage.vue'
  import type { IRSUserControlEvents } from '../Interfaces/IRSUserControlEvents';
  const RSMessageEvents = ref<InstanceType<typeof RSMessage> | null>(null);

  function ShowDangerMsg(msg: string): void {
    RSMessageEvents.value?.ShowDangerMsg(msg);
  }

  function ShowDarkMsg(msg: string): void {
    RSMessageEvents.value?.ShowDarkMsg(msg);
  }

  function ShowDismissibleMsg(msg: string): void {
    RSMessageEvents.value?.ShowDismissibleMsg(msg);
  }

  function ShowHeadingMsg(msg: string): void {
    RSMessageEvents.value?.ShowHeadingMsg(msg);
  }


  function ShowInfoMsg(msg: string): void {
    RSMessageEvents.value?.ShowInfoMsg(msg);
  }


  function ShowLightMsg(msg: string): void {
    RSMessageEvents.value?.ShowLightMsg(msg);
  }


  function ShowLinkMsg(msg: string): void {
    RSMessageEvents.value?.ShowLinkMsg(msg);
  }


  function ShowPrimaryMsg(msg: string): void {
    RSMessageEvents.value?.ShowPrimaryMsg(msg);
  }


  function ShowSecondaryMsg(msg: string): void {
    RSMessageEvents.value?.ShowSecondaryMsg(msg);
  }


  function ShowSuccessMsg(msg: string): void {
    RSMessageEvents.value?.ShowSuccessMsg(msg);
  }


  function ShowWarningMsg(msg: string): void {
    RSMessageEvents.value?.ShowWarningMsg(msg);
  }

  function ClearMsg(): void {
    RSMessageEvents.value?.ClearMsg();
  }

  function ShowMsg(type: string, msg: string): void {
    RSMessageEvents.value?.ShowMsg(type, msg);
  }

  // 暴露方法给父组件
  defineExpose<IRSUserControlEvents>({
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


