//// 导入Bootstrap CSS
//import 'bootstrap/dist/css/bootstrap.min.css'
//// 导入Bootstrap JavaScript
//import 'bootstrap/dist/js/bootstrap.bundle.min.js'
//// 导入Bootstrap Icons
//import 'bootstrap-icons/font/bootstrap-icons.css'

import "bootstrap"

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')
