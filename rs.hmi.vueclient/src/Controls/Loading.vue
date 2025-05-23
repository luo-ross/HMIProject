<template>
  <div class="loading">
    <slot></slot>

    <div v-if="LoadingType === 'ProgressBar'" class="loading-border"
         :class="{'d-none': !IsLoading }">
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
          {{ LoadingText  }}
        </div>
      </div>
    </div>
    <div v-else-if="LoadingType === 'Rotate'"
         class="loading-border rotate-loading"
         :class="{'d-none': !IsLoading }">
      <IconLoading FillColor="#1296db" class="icon-loading  circle-rotate-loading circle-rotate-progress-animated"></IconLoading>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, onUnmounted, watch, computed } from 'vue';
  import type { Func } from '../Events/Func'
  import type { ILoadingEvents } from '../Interfaces/ILoadingEvents'
  import { GenericOperateResult } from '../Commons/OperateResult/OperateResult'
  import IconLoading from '../Controls/Icons/IconLoading.vue'
  import { LoadingEnum } from '../Commons/Enums/LoadingEnum'
  const LoadingType = defineModel('LoadingType', {
    default: 'Rotate'
  });

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


  async function GenericLoadingActionAsync<T> (func: Func<Promise<GenericOperateResult<T>>>)
    : Promise<GenericOperateResult<T>> {
    try {
      IsLoading.value = true;
      const operateResult = await func?.();
      IsLoading.value = false;
      return operateResult ?? GenericOperateResult.CreateFailResult<T>("操作失败");
    } catch(e) {
      IsLoading.value = false;
      return GenericOperateResult.CreateFailResult<T>(e instanceof Error ? e.message : "操作失败");
    }
  }

  async function SimpleLoadingActionAsync<T>(func: Func<Promise<GenericOperateResult<T>>>)
    : Promise<GenericOperateResult<T>> {
    try {
      IsLoading.value = true;
      const operateResult = await func?.();
      IsLoading.value = false;
      return operateResult ?? GenericOperateResult.CreateFailResult<T>("操作失败");
    } catch (e) {
      IsLoading.value = false;
      return GenericOperateResult.CreateFailResult<T>(e instanceof Error ? e.message : "操作失败");
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
    GenericLoadingActionAsync,
    SimpleLoadingActionAsync
  });
</script>


