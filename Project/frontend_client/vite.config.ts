import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path'
import { API_BASE_URL } from './config'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
      port: 5173,
      //open: true,
      proxy: {
          '/api': {
              target: API_BASE_URL,
              changeOrigin: true,
              secure: false,
          }
      }
  },
  build: {
    outDir: path.resolve(__dirname, '../Backend_Server/wwwroot'),
    emptyOutDir: true, // Clears the output directory before each build
    assetsDir: 'assets',
  }
})
