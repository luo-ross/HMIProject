import "bootstrap"

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import { RouterUtil } from "./Commons/Network/RouterUtil"

const app = createApp(App)
app.use(createPinia())
app.use(RouterUtil.GetInstance().GetRouter())
app.mount('#app')
