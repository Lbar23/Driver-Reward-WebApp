import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path'
const API_BASE_URL = process.env.NODE_ENV === 'production'
  ? 'http://localhost:5000'  // prod url
  : 'http://localhost:5041'; // dev url

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
              secure: true,
          }
      }
  },
  build: {
    outDir: path.resolve(__dirname, '../Backend_Server/wwwroot'),
    emptyOutDir: true, // Clears the output directory before each build
    assetsDir: 'assets',
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            // Create separate chunks for large libraries
            if (id.includes('@mui')) return 'mui';
            return 'vendor';
          }
        }
      }
    },
  }
})
