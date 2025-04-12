<template>
  <div class="form-row">
    <div class="form-input-border">
      <svg t="1743616005570" class="svg-icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="15425" width="64" height="64"><path d="M864.930549 169.353453 159.069451 169.353453C107.495953 169.353453 65.541321 211.308084 65.541321 262.881583l0 498.236834c0 51.573499 41.954632 93.52813 93.52813 93.52813l705.861097 0c51.573499 0 93.52813-41.954632 93.52813-93.52813L958.458679 262.881583C958.458679 211.308084 916.504047 169.353453 864.930549 169.353453zM127.859298 285.700809l209.363845 202.712501L127.859298 739.527131 127.859298 285.700809zM635.305686 459.045068c-64.773858 64.773858-171.093035 65.694814-236.992505 1.944239L161.627661 231.67143 862.679324 231.67143 635.305686 459.045068zM384.089537 529.549315c39.396423 27.219346 85.546517 40.82902 131.798941 40.82902 42.466274 0 84.932547-11.563106 122.077746-34.484661l220.926951 256.434896L164.902168 792.32857 384.089537 529.549315zM686.162886 496.292595 896.24303 286.212451l0 453.92865L686.162886 496.292595z" p-id="15426"></path></svg>
      <input ref="EmailElement"
             autocomplete="off"
             type="email"
             maxlength="50"
             class="form-input form-input-email"
             :placeholder="Placeholder"
             v-model="Email" />
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed } from 'vue'
  import { IEvents } from '../Interfaces/IEvents'
  const Email = defineModel('Email', {
    type: String,
  });
  const Placeholder = defineModel('Placeholder', {
    type: String,
    default: '请输入邮箱'
  });
  // 创建输入框的引用
  const EmailElement = ref<HTMLInputElement | null>(null);
  // 添加获取焦点的方法
  const Focus = () => {
    if (EmailElement.value) {
      EmailElement.value.focus();
    }
  };
  // 暴露方法给父组件
  defineExpose<IEvents>({
    Focus
  });
</script>
