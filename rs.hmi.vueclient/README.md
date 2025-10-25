������������������������������������������������������������ Vue��Ŀָ�� ����������������������������������������������������������
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
������������������������������������������������������ Vue��Ŀָ�� ������������������������������������������������������������������������



������������������������������������������������������Vue��ĿIIS����ָ�� ����������������������������������������������������������

## ����
��ָ����ϸ˵����ν�Vue��ҳӦ��(SPA)����IIS�����������·��404���⡣

## ǰ������
- Windows Server �� Windows 10/11
- IIS �Ѱ�װ
- IIS URL Rewrite Module �Ѱ�װ

## һ����װIIS URL Rewrite Module

### 1. ���ص�ַ
- �ٷ����أ�https://www.iis.net/downloads/microsoft/url-rewrite
- ��ͨ�� Web Platform Installer ��װ

### 2. ��װ����
1. ���� `rewrite_amd64_en-US.msi`
2. ˫����װ
3. ����IIS����

## ����Vue��Ŀ����

### 1. Vite���� (vite.config.ts)
```typescript
export default defineConfig(({ mode }) => {
  return {
    // ��������
    build: {
      outDir: 'dist',        // ���Ŀ¼
      assetsDir: 'assets',   // ��̬��ԴĿ¼
    },
    
    // ��������...
  }
})
```

### 2. ������Ŀ
```bash
# ������������
npm run build:prod

# ��
npm run build
```

## ����IIS����

### ����һ��ͨ��IIS���������ã��Ƽ���

#### 1. ��IIS������
- �ҵ�������վ
- ˫��"URL��д"����

#### 2. ��ӿհ׹���
- ����Ҳ�"��ӹ���"
- ѡ��"��վ����" �� "�հ׹���"

#### 3. ���ù�������

**�������ƣ�**
```
Vue Router History Mode
```

**ƥ��URL��**
- **ģʽ**��`.*`
- **���Դ�Сд**����ѡ

**���������������������**

**����1��**
- **����**��`{REQUEST_FILENAME}`
- **����**��`�����ļ�`
- **ģʽ**������

**����2��**
- **����**��`{REQUEST_FILENAME}`
- **����**��`����Ŀ¼`
- **ģʽ**������

**������**
- **��������**��`��д`
- **��дURL**��`/index.html`
- **���Ӳ�ѯ�ַ���**����ѡ

#### 4. �������
���"Ӧ��"�������

### ��������ʹ��web.config�ļ�

#### 1. ����web.config�ļ�
����վ��Ŀ¼���� `web.config` �ļ���

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- Vue Router Historyģʽ֧�� -->
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

## �ġ�������

### 1. ������Ŀ
```bash
npm run build:prod
```

### 2. �����ļ�
�� `dist` �ļ����е��������ݸ��Ƶ�IIS��վ��Ŀ¼

### 3. ����URL��д
������������һ�򷽷�������URL��д����

### 4. ����IISվ��
- ��IIS���������Ҽ����������վ
- ѡ��"��������"


## �塢�����߼�˵��

URL��д��������ã�
- **�����ļ�** AND **����Ŀ¼** = ����·��
- ������·����д�� `/index.html`
- ��Vue Router����ͻ���·��

### ����д�������
- `/EmailPasswordReset` - ����·��
- `/Home` - ����·��
- `/Login` - ����·��

### ������д�������
- `/index.html` - ʵ���ļ�
- `/assets/app.js` - ʵ���ļ�
- `/images/logo.png` - ʵ���ļ�

������������������������������������������������������ Vue��ĿIIS����ָ�� ������������������������������������������������������������������������

