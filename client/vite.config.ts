/// <reference types="vitest/config" />
import { ServerResponse } from 'node:http'
import { fileURLToPath, URL } from 'node:url'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv, type ProxyOptions } from 'vite'

const projectRoot = fileURLToPath(new URL('.', import.meta.url))

/**
 * Proxy to the API. While the API is down (still compiling, or `dotnet watch` restarting it) the browser gets
 * a 503 ProblemDetails, the same error shape the API itself returns, instead of a generic 500. The app's
 * retry policy treats 5xx as transient, so the UI recovers on its own once the API is back.
 */
function apiProxy(target: string): ProxyOptions {
  return {
    target,
    changeOrigin: true,
    configure: (proxy) => {
      proxy.on('error', (error, request, response) => {
        console.warn(
          `[proxy] API unreachable at ${target} for ${request.url ?? ''} (${error.message || 'connection failed'})`,
        )
        if (!(response instanceof ServerResponse) || response.headersSent) return
        response.writeHead(503, { 'Content-Type': 'application/problem+json', 'Retry-After': '2' })
        response.end(
          JSON.stringify({
            title: 'API unavailable',
            status: 503,
            detail: `The development proxy could not reach the API at ${target}. Is it running?`,
          }),
        )
      })
    },
  }
}

export default defineConfig(({ mode }) => {
  // Vite does not put .env values into process.env for its own config, so load them explicitly.
  // The '' prefix loads every variable, including non-VITE_ ones like API_URL that never reach the browser.
  const env = loadEnv(mode, projectRoot, '')
  // 127.0.0.1, not localhost: Node tries IPv6 ::1 first for "localhost", which adds a failed attempt per request.
  const apiUrl = env.API_URL ?? 'http://127.0.0.1:5001'

  return {
    plugins: [react()],
    resolve: {
      // Keep in sync with "paths" in tsconfig.app.json.
      alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) },
    },
    server: {
      // Explicit IPv4 loopback. "localhost" makes Node 17+ on Windows listen on ::1 only, which tools that
      // connect to 127.0.0.1 (test runners, WSL, some proxies) cannot reach. Browsers fall back to it fine.
      host: '127.0.0.1',
      port: 5173,
      strictPort: true,
      // Same-origin calls in development, exactly as in production where nginx proxies the API.
      proxy: {
        '/api': apiProxy(apiUrl),
        '/health': apiProxy(apiUrl),
      },
    },
    build: {
      sourcemap: false,
      rolldownOptions: {
        output: {
          // Vendor code changes rarely; separate chunks stay cached in browsers across app deploys.
          codeSplitting: {
            groups: [
              { name: 'vendor-mui', test: /node_modules[\\/](@mui|@emotion)[\\/]/ },
              { name: 'vendor-react', test: /node_modules[\\/](react|react-dom|scheduler)[\\/]/ },
            ],
          },
        },
      },
    },
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      restoreMocks: true,
    },
  }
})
