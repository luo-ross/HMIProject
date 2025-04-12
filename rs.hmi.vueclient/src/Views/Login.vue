<template>
  <div class="login-container">
    <RSEmail 
      ref="viewModel.EmailRef"
      v-model:Email="viewModel.Email"
      :Placeholder="'请输入邮箱'"
    />
    <!-- ... 其他组件 ... -->
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useLoginViewModel } from '../ViewModels/LoginViewModel';
import RSEmail from '../Controls/RSEmail.vue';

const viewModel = useLoginViewModel();

onMounted(() => {
  // 自动获取焦点
  viewModel.FocusEmail();
});
</script> 