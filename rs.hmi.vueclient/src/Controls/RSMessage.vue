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
  //3.4以后版本使用
  const Message = defineModel('Message');
  const MessageType = defineModel('MessageType');
</script>
