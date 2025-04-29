<style>
  .home-main {
    width:100%;
    height:100%;
    display:flex;
  }
</style>
<template>
  <div class="home-main">
    <Message :ref="events => ViewModel.MessageEvents= events"></Message>
    <Loading LoadingType="ProgressBar" :ref="events => ViewModel.LoadingEvents= events">
      <div>
        <div class="home-left">
          <div></div>
          <div>
            <ul>

            </ul>
          </div>
        </div>
        <div class="home-right">
          <div class="caption"></div>
          <div class="content"></div>
        </div>
      </div>
    </Loading>
  </div>
</template>

<script setup lang="ts">
  import { ref, watch, computed } from 'vue'
  import Loading from '../../Controls/Loading.vue'
  import Message from '../../Controls/Message.vue'
  import { HomeViewModel } from './HomeViewModel'
  const ViewModel = new HomeViewModel()
</script>
