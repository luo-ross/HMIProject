import { defineConfig, loadEnv } from 'vite';
import plugin from '@vitejs/plugin-vue';
import { resolve } from 'path';

// Vite配置文件
// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  // 加载环境变量
  // mode: 当前运行模式（development/production）
  // process.cwd(): 当前工作目录
  const env = loadEnv(mode, process.cwd());
  
  // 设置API目标地址
  // 优先级：ASPNETCORE_HTTPS_PORT > ASPNETCORE_URLS > 默认值
  const target = env.ASPNETCORE_HTTPS_PORT ? `http://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:7109';

  return {
  
    // 插件配置
    plugins: [plugin()],  // 使用Vue插件

    // 开发服务器配置
    server: {
      // 设置开发服务器端口
      // 优先使用环境变量中的端口，如果没有则使用默认值54293
      port: parseInt(env.DEV_SERVER_PORT || '54293'),
      
      // 禁用缓存，避免开发时的缓存问题
      headers: {
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0'
      },
      
      // 代理配置，用于解决开发环境的跨域问题
      proxy: {
        // 当请求路径以 /api/v1/ 开头时，会被代理到target指定的地址
        '/api/v1/': {
          target,              // 代理目标地址
          secure: false,       // 是否验证SSL证书
          changeOrigin: true,  // 是否改变源地址，解决跨域问题
        }
      },
    },

    // 构建配置
    build: {
      // Rollup打包配置
      rollupOptions: {
        input: {
          main: resolve(__dirname, 'index.html'),  // 入口文件
        },
        output: {
          // 为静态资源添加hash，避免缓存问题
          assetFileNames: 'assets/[name]-[hash][extname]',
          chunkFileNames: 'assets/[name]-[hash].js',
          entryFileNames: 'assets/[name]-[hash].js'
        }
      },
      outDir: 'dist',        // 输出目录
      assetsDir: 'assets',   // 静态资源目录

      // 生产环境优化配置
      minify: 'terser',      // 使用terser进行代码压缩
      terserOptions: {
        compress: {
          drop_console: false,    // 移除console语句
          drop_debugger: false,   // 移除debugger语句
        },
      },
    },

    // 路径解析配置
    resolve: {
      alias: {
        '@': resolve(__dirname, 'src'),  // 设置@别名指向src目录
      },
    },

    // 静态资源目录配置
    publicDir: 'public',  // 静态资源目录，该目录下的文件会被原样复制到输出目录
  }
})
