//import { fileURLToPath, URL } from 'node:url';

//import { defineConfig } from 'vite';
//import plugin from '@vitejs/plugin-react';
//import fs from 'fs';
//import path from 'path';
//import child_process from 'child_process';
//import { env } from 'process';

//const baseFolder =
//    env.APPDATA !== undefined && env.APPDATA !== ''
//        ? `${env.APPDATA}/ASP.NET/https`
//        : `${env.HOME}/.aspnet/https`;

//const certificateName = "team16-webapp-4910.client";
//const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
//const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

//if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
//    if (0 !== child_process.spawnSync('dotnet', [
//        'dev-certs',
//        'https',
//        '--export-path',
//        certFilePath,
//        '--format',
//        'Pem',
//        '--no-password',
//    ], { stdio: 'inherit', }).status) {
//        throw new Error("Could not create certificate.");
//    }
//}

//const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
//    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7284';

//// https://vitejs.dev/config/
//export default defineConfig({
//    plugins: [plugin()],
//    resolve: {
//        alias: {
//            '@': fileURLToPath(new URL('./src', import.meta.url))
//        }
//    },
//    server: {
//        proxy: {
//            '/api': {
//                target,
//                secure: false
//            }
//        },
//        port: 5173,
//        https: {
//            key: fs.readFileSync(keyFilePath),
//            cert: fs.readFileSync(certFilePath),
//        },
//        open: true
//    }
//})

//import { fileURLToPath, URL } from 'node:url';
//import { defineConfig } from 'vite';
//import react from '@vitejs/plugin-react';

//// https://vitejs.dev/config/
//export default defineConfig({
//    plugins: [react()],
//    resolve: {
//        alias: {
//            '@': fileURLToPath(new URL('./src', import.meta.url))
//        }
//    },
//    server: {
//        port: 5173,
//        open: true
//    }
//})

import { defineConfig } from 'vite'
import tsconfigPaths from 'vite-tsconfig-paths';
import react from '@vitejs/plugin-react'
import tailwindcss from 'tailwindcss'
import autoprefixer from 'autoprefixer'

export default defineConfig({
    plugins: [react(), tsconfigPaths(), ],
    server: {
        port: 5173,
        open: true,
        proxy: {
            '/api': {
                //target: 'https://localhost:7284',
                target: 'http://localhost:5062',
                changeOrigin: true,
                secure: false,
            }
        }
    },
    css: {
        postcss: {
            plugins: [
                tailwindcss,
                autoprefixer,
            ],
        },
    },
})
