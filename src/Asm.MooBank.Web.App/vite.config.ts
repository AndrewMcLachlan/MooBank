import { PluginOption, defineConfig, splitVendorChunkPlugin } from "vite"
import { visualizer } from "rollup-plugin-visualizer";
import react from "@vitejs/plugin-react"
import svgr from "vite-plugin-svgr";
import tsconfigpaths from "vite-tsconfig-paths";
import { fileURLToPath } from "url"

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [tsconfigpaths(), svgr(), react(), visualizer() as any],
  server: {
    port: 3005,
    proxy: {
      "/api": "http://localhost:51579"
    }
  },
  resolve: {
    alias: {
      "~": fileURLToPath(new URL("node_modules/", import.meta.url)),
    }
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
