import './assets/main.css'
// Element Plus CSS will be auto-imported by unplugin-vue-components

import { createApp } from 'vue'
import { createPinia } from 'pinia'
// ElementPlus will be auto-imported by unplugin-vue-components
import App from './App.vue'
import { router } from './router'
import { installHostBridge } from './host/terminalBridge'

installHostBridge()

const app = createApp(App)

app.use(createPinia())
app.use(router)
// ElementPlus components will be auto-imported

app.mount('#app')
