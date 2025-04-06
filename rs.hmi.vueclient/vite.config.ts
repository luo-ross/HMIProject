import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';
import { resolve } from 'path';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 54293,
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
