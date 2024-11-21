import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc';
import path from 'path';

const API_BASE_URL = process.env.NODE_ENV === 'production'
  ? 'http://localhost:5000'  // Production URL
  : 'http://localhost:5041'; // Development URL

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
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
    emptyOutDir: true,
    assetsDir: 'assets',
    sourcemap: false, // Disable source maps for production to reduce build size
    minify: 'esbuild', // Use faster minifier
    rollupOptions: {
      output: {
        manualChunks: {
          // Core React libraries
          vendor: ['react', 'react-dom', 'react-router-dom'],

          // // pdf components
          // pdf: ['jspdf', 'jspdf-autotable'],

          // Core MUI components
          'mui-core': [
            '@mui/material/Button',
            '@mui/material/Card',
            '@mui/material/Dialog',
            '@mui/material/Typography',
            '@mui/material/Input',
          ],

          // MUI data and visualization
          'mui-data-visualization': [
            '@mui/x-charts',
            '@mui/x-data-grid',
            '@mui/x-date-pickers',
          ],

          // MUI icons
          'mui-icons': ['@mui/icons-material'],

          // Utility libraries
          utilities: ['axios', 'date-fns'],
        },
      },
    },
    chunkSizeWarningLimit: 500,
  },
  esbuild: {
    drop: process.env.NODE_ENV === 'production' ? ['console', 'debugger'] : [],
  },
  optimizeDeps: {
    include: ["@emotion/react", "@emotion/styled"]
  },
  ssr: {
    noExternal: ["@emotion/react", "@emotion/styled"]
  }
  
});
