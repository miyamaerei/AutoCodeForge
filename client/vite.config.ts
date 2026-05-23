import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import Components from 'unplugin-vue-components/vite'
import AutoImport from 'unplugin-auto-import/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

function getElementPlusChunk(id: string): string {
  const componentMatch = id.match(/element-plus[\\/]es[\\/]components[\\/]([^\\/]+)/)
  if (componentMatch?.[1]) {
    return `vendor-el-${componentMatch[1]}`
  }

  if (id.includes('element-plus/es/hooks')) {
    return 'vendor-el-hooks'
  }

  if (id.includes('element-plus/es/utils')) {
    return 'vendor-el-utils'
  }

  if (id.includes('element-plus/es/tokens')) {
    return 'vendor-el-tokens'
  }

  if (id.includes('element-plus/es/directives')) {
    return 'vendor-el-directives'
  }

  return 'vendor-element-plus-core'
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    // Only enable Vue DevTools in development mode
    process.env.NODE_ENV === 'development' ? vueDevTools() : null,
    AutoImport({
      resolvers: [ElementPlusResolver()],
    }),
    Components({
      resolvers: [ElementPlusResolver()],
    }),
  ].filter(Boolean),
  build: {
    // Disable sourcemaps in production
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (!id.includes('node_modules')) {
            return undefined
          }

          if (id.includes('vue-echarts')) {
            return 'vendor-vue-echarts'
          }

          if (id.includes('zrender')) {
            return 'vendor-zrender'
          }

          if (id.match(/echarts[\\/](?:lib[\\/])?charts?[\\/]/)) {
            return 'vendor-echarts-charts'
          }

          if (id.match(/echarts[\\/](?:lib[\\/])?components?[\\/]/)) {
            return 'vendor-echarts-components'
          }

          if (id.match(/echarts[\\/](?:lib[\\/])?renderers?[\\/]/)) {
            return 'vendor-echarts-renderers'
          }

          if (id.match(/echarts[\\/](?:lib[\\/])?(?:core|coord|data|layout|model|processor|scale|util|view)[\\/]/)) {
            return 'vendor-echarts-internals'
          }

          if (id.includes('echarts')) {
            return 'vendor-echarts-core'
          }

          if (id.includes('element-plus') || id.includes('@element-plus')) {
            return getElementPlusChunk(id)
          }

          if (id.includes('vue-router') || id.includes('pinia') || id.includes('/vue/')) {
            return 'vendor-vue'
          }

          if (id.includes('axios')) {
            return 'vendor-axios'
          }

          if (id.includes('markdown-it')) {
            return 'vendor-markdown'
          }

          return 'vendor-misc'
        },
      },
    },
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5254',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req) => {
            // Ensure all headers are forwarded
            Object.keys(req.headers).forEach((key) => {
              const value = req.headers[key];
              if (value !== undefined) {
                proxyReq.setHeader(key, value);
              }
            });
          });
        },
      },
    },
  },
})
