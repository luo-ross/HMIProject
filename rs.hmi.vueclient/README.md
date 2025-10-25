↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓ Vue项目指南 ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
# rs.hmi.vueclient

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Type-Check, Compile and Minify for Production

```sh
npm run build
```

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```
↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑ Vue项目指南 ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑



↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓Vue项目IIS部署指南 ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

## 概述
本指南详细说明如何将Vue单页应用(SPA)部署到IIS服务器，解决路由404问题。

## 前置条件
- Windows Server 或 Windows 10/11
- IIS 已安装
- IIS URL Rewrite Module 已安装

## 一、安装IIS URL Rewrite Module

### 1. 下载地址
- 官方下载：https://www.iis.net/downloads/microsoft/url-rewrite
- 或通过 Web Platform Installer 安装

### 2. 安装步骤
1. 下载 `rewrite_amd64_en-US.msi`
2. 双击安装
3. 重启IIS服务

## 二、Vue项目配置

### 1. Vite配置 (vite.config.ts)
```typescript
export default defineConfig(({ mode }) => {
  return {
    // 构建配置
    build: {
      outDir: 'dist',        // 输出目录
      assetsDir: 'assets',   // 静态资源目录
    },
    
    // 其他配置...
  }
})
```

### 2. 构建项目
```bash
# 生产环境构建
npm run build:prod

# 或
npm run build
```

## 三、IIS配置

### 方法一：通过IIS管理器配置（推荐）

#### 1. 打开IIS管理器
- 找到您的网站
- 双击"URL重写"功能

#### 2. 添加空白规则
- 点击右侧"添加规则"
- 选择"入站规则" → "空白规则"

#### 3. 配置规则详情

**规则名称：**
```
Vue Router History Mode
```

**匹配URL：**
- **模式**：`.*`
- **忽略大小写**：勾选

**条件（添加两个条件）：**

**条件1：**
- **输入**：`{REQUEST_FILENAME}`
- **类型**：`不是文件`
- **模式**：留空

**条件2：**
- **输入**：`{REQUEST_FILENAME}`
- **类型**：`不是目录`
- **模式**：留空

**操作：**
- **操作类型**：`重写`
- **重写URL**：`/index.html`
- **附加查询字符串**：勾选

#### 4. 保存规则
点击"应用"保存规则

### 方法二：使用web.config文件

#### 1. 创建web.config文件
在网站根目录创建 `web.config` 文件：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- Vue Router History模式支持 -->
        <rule name="Vue Router" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

## 四、部署步骤

### 1. 构建项目
```bash
npm run build:prod
```

### 2. 复制文件
将 `dist` 文件夹中的所有内容复制到IIS网站根目录

### 3. 配置URL重写
按照上述方法一或方法二配置URL重写规则

### 4. 重启IIS站点
- 在IIS管理器中右键点击您的网站
- 选择"重新启动"


## 五、规则逻辑说明

URL重写规则的作用：
- **不是文件** AND **不是目录** = 虚拟路径
- 将虚拟路径重写到 `/index.html`
- 让Vue Router处理客户端路由

### 会重写的情况：
- `/EmailPasswordReset` - 虚拟路由
- `/Home` - 虚拟路由
- `/Login` - 虚拟路由

### 不会重写的情况：
- `/index.html` - 实际文件
- `/assets/app.js` - 实际文件
- `/images/logo.png` - 实际文件

↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑ Vue项目IIS部署指南 ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

