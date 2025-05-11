/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_APP_ENV: string
  readonly VITE_APP_BASE_API: string
  readonly VITE_APP_TITLE: string
  readonly VITE_APP_DEBUG: boolean
  readonly VITE_APP_SECRET_KEY: string
  readonly VITE_APP_API_KEY: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
} 