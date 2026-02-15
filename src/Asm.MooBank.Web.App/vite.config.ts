import { defineConfig } from "vite"
import { visualizer } from "rollup-plugin-visualizer";
import react from "@vitejs/plugin-react"
import svgr from "vite-plugin-svgr";
import tsconfigpaths from "vite-tsconfig-paths";
import { fileURLToPath } from "url"

import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "localhost";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [tsconfigpaths(),   
        svgr({
            svgrOptions: {
            plugins: ["@svgr/plugin-svgo", "@svgr/plugin-jsx"],
            svgoConfig: {
                plugins: [{
                name: "preset-default",
                params: { overrides: { removeViewBox: false, cleanupIds: false } },
                }],
            },
            },
            include: "**/*.svg",
        }), 
        react(), visualizer() as any],
    server: {
        port: 3005,
        proxy: {
            "/api": {
                // Use environment variable for proxy target to avoid hard-coding IP addresses
                target: env.VITE_API_PROXY_TARGET || "http://localhost:5005",
                changeOrigin: true,
                secure: false,
            }
        },
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    },
    resolve: {
        alias: {
            "~": fileURLToPath(new URL("node_modules/", import.meta.url)),
        },
        dedupe: [
            'react',
            'react-dom',
        ]
    },
    build: {
        rollupOptions: {
            output: {
                manualChunks(id: string) {
                    if (id.includes("msal")) {
                        return "@msal";
                    }
                    if (id.includes("react-router") || id.includes("@remix-run")) {
                        return "@react-router";
                    }
                    if (id.includes("chart.js")) {
                        return "@chart.js";
                    }
                    if (id.includes("react-select") || id.includes("@fortawesome")) {
                        return "@react-select-and-fontawesome";
                    }
                    if (id.includes("date-fns") || id.includes("axios")) {
                        return "@utlities";
                    }
                }
            }
        }
    }
})
