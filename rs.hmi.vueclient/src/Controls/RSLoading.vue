
<template>
  <div class="loading">
    <slot></slot>
    <div class="progress-border"
         :class="{'d-none': !IsLoading }"
         >
      <div class="progress">
        <div v-if="!IsAutoIncrement"
             class="progress-bar progress-bar-animated progress-bar-indeterminate"
             role="progressbar"
             aria-valuemin="0"
             aria-valuemax="100">
        </div>
        <div v-else
             class="progress-bar progress-bar-animated"
             :style="{ width: ProgressValue + '%' }"
             role="progressbar"
             :aria-valuenow="ProgressValue"
             aria-valuemin="0"
             aria-valuemax="100">
        </div>

        <div v-if="IsShowText" class="loading-text">
          {{ text || '加载中...' }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, onUnmounted, watch, computed } from 'vue';
  import { Func } from '../Events/Func'
  import { ILoadingEvents } from '../Interfaces/ILoadingEvents'
  import { SimpleOperateResult } from '../Commons/OperateResult/OperateResult'

  const IsLoading = defineModel('IsLoading', {
    type: Boolean,
    default: false
  });

  const ProgressValue = defineModel('ProgressValue', {
    type: Number,
    default: 0
  });

  const IsShowText = defineModel('IsShowText', {
    type: Boolean,
    default: false
  });

  const LoadingText = defineModel('LoadingText', {
    type: String,
    default: '正在加载中'
  });

  const IsAutoIncrement = defineModel('IsAutoIncrement', {
    type: Boolean,
    default: false
  });

  const IsIncrementInterval = defineModel('IsIncrementInterval', {
    type: Number,
    default: 1
  });


  async function InvokeLoadingActionAsync(func: Func<Promise<SimpleOperateResult>>): Promis<SimpleOperateResult> {

    try {

      IsLoading.value = true;

    
      const operateResult = await func?.();

      IsLoading.value = false;
      return operateResult;
    } catch (e) {

    }
  }

  
  //// 定义 Test 事件
  //const emit = defineEmits<{
  //  (e: 'test', value: string): void
  //}>();

  //// 触发 Test 事件的方法
  //const triggerTest = (value: string) => {
  //  emit('test', value);
  //};
  // 导出方法供父组件调用
  defineExpose<ILoadingEvents>({
    InvokeLoadingActionAsync
  });
</script>


