import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Proxy API calls to the ASP.NET Core backend during development.
    // This avoids CORS issues for the /api prefix in development mode.
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        // Strip the /api prefix so the backend sees e.g. /{guid}/notes
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
})

