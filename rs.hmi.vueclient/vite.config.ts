import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';
import { resolve } from 'path';
import { env } from 'process';

const target = env.ASPNETCORE_HTTPS_PORT ? `http://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:7109';


// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin()],
  server: {
    port: parseInt(env.DEV_SERVER_PORT || '54293'),
    proxy: {
      '/api/v1/': {
        target,
        secure: false,
        changeOrigin: true,
      }
    },
  },
  build: {
    rollupOptions: {
      input: {
        main: resolve(__dirname, 'index.html'),
      },
    },
    outDir: 'dist',
    assetsDir: 'assets',
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  // 配置静态资源目录
  publicDir: 'public',

})
