import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path'
const API_BASE_URL = process.env.NODE_ENV === 'production'
  ? 'http://localhost:5000'  // prod url
  : 'http://localhost:5041'; // dev url

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react()
  ],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: API_BASE_URL,
        changeOrigin: true,
        secure: process.env.NODE_ENV === 'production', // Secure proxy only in production
      }
    }
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'), // Simplifies imports with '@' prefix
    }
  },
  build: {
    outDir: path.resolve(__dirname, '../Backend_Server/wwwroot'),
    emptyOutDir: true,
    assetsDir: 'assets',
    sourcemap: process.env.NODE_ENV !== 'production', // Only generate source maps in development
    minify: 'esbuild', // Faster minification
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            // Specific chunks for commonly used MUI components
            if (id.includes('@mui/material/Button')) {
              return 'mui-button';
            }
            if (id.includes('@mui/material/Card')) {
              return 'mui-card';
            }
            if (id.includes('@mui/material/Alert')) {
              return 'mui-alert';
            }
            if (id.includes('@mui/material/Typography')) {
              return 'mui-typography';
            }
            if (id.includes('@mui/material/Grid')) {
              return 'mui-grid';
            }
            if (id.includes('@mui/material/TextField')) {
              return 'mui-textfield';
            }
            if (id.includes('@mui/icons-material')) {
              return 'mui-icons';
            }

            // Groups other parts of @mui/material into a base chunk
            if (id.includes('@mui/material')) {
              return 'mui-material';
            }

            // Split other large dependencies
            if (id.includes('react')) {
              return 'vendor-react';
            }

            // Default chunking for all other node_modules
            return id.toString().split('node_modules/')[1].split('/')[0];

          }
        },
      },
    },
    chunkSizeWarningLimit: 500, // Set warning limit for chunk sizes
  },
  esbuild: {
    drop: process.env.NODE_ENV === 'production' ? ['console', 'debugger'] : [], // Remove console and debugger in production
  }
});
