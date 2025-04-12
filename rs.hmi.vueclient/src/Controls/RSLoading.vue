<style scoped>
  .loading {
    display: flex;
    width: 100%;
    flex-direction: column;
    position: relative;
  }

  .progress-border {
    display: flex;
    width: 100%;
    position: absolute;
    height:100%;
    background-color:#00000008;
  }

    .progress {
      width: 100%;
      position: absolute;
      justify-content: center;
      align-items: center;
      min-height: 2px;
      left: 0px;
      top: 0px;
      right: 0px;
      border-radius: 0px;
      height: auto;
    }

  .progress-bar {
    height: 100%;
    background-color: #0d6efd;
    transition: width 0.6s ease;
    position: absolute;
    left: 0;
    top: 0;
  }


  .progress-bar-animated {
    animation: progress-bar-stripes 1s linear infinite;
  }

  .progress-bar-indeterminate {
    width: 30%;
    animation: indeterminate-progress 1.5s ease-in-out infinite;
  }

  @keyframes progress-bar-stripes {
    from {
      background-position: 1rem 0;
    }

    to {
      background-position: 0 0;
    }
  }

  @keyframes indeterminate-progress {
    0% {
      left: -30%;
    }

    100% {
      left: 100%;
    }
  }

  .loading-text {
    color: #6c757d;
    font-size: 0.875rem;
    padding: 2px 0px 2px 0px;
  }
</style>
<template>
  <div class="loading">
    <slot></slot>
    <div class="progress-border  d-none">

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



  //// 定义 Test 事件
  //const emit = defineEmits<{
  //  (e: 'test', value: string): void
  //}>();

  //// 触发 Test 事件的方法
  //const triggerTest = (value: string) => {
  //  emit('test', value);
  //};
  //// 导出方法供父组件调用
  //defineExpose({
  //  triggerTest
  //});
</script>


