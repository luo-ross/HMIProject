
<template>
  <Border>
    <RSMessage ref="RSMessageEvents"
               v-model:Message="Message"
               v-model:MessageType="MessageType"></RSMessage>
    <slot></slot>
  </Border>
</template>

<script setup lang="ts">

</script>


