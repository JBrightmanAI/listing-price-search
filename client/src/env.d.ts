// Types for import.meta.env. Only VITE_-prefixed variables reach the browser; see .env.example.
interface ImportMetaEnv {
  /** API origin when it differs from the UI's. Empty or unset means same origin. */
  readonly VITE_API_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
