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
        manualChunks(id) {
          if (id.includes('node_modules')) {
            // Separate large libraries from @mui/material and others
            if (id.includes('@mui/material/Button')) return 'mui-button';
            if (id.includes('@mui/material/Card')) return 'mui-card';
            if (id.includes('@mui/material/Dialog')) return 'mui-dialog';
            if (id.includes('@mui/material/Grid')) return 'mui-grid';
            if (id.includes('@mui/material/Typography')) return 'mui-typography';
            if (id.includes('@mui/material/Input')) return 'mui-input';
            if (id.includes('@mui/material/Tooltip')) return 'mui-tooltip';

            // Split `@mui/icons-material` separately
            if (id.includes('@mui/icons-material')) return 'mui-icons';

            // Group x-charts, x-date-pickers, and other MUI extensions separately..
            if (id.includes('@mui/x-charts')) return 'mui-charts';
            if (id.includes('@mui/x-date-pickers')) return 'mui-date-pickers';

            // Fallback for remaining Material components
            if (id.includes('@mui/material')) return 'mui-material';

            // Separate other large dependencies like React and D3
            if (id.includes('react')) return 'vendor-react';
            if (id.includes('d3')) return 'vendor-d3';
            if (id.includes('axios')) return 'vendor-axios';

            // Fallback for remaining node_modules dependencies
            return 'vendor';
          }
        }
      }
    },
    chunkSizeWarningLimit: 500, // Set warning limit for large chunks (don't ignore it!)
  },
  esbuild: {
    drop: process.env.NODE_ENV === 'production' ? ['console', 'debugger'] : [],
  }
});
