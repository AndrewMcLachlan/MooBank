import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"
import svgr from "vite-plugin-svgr";
import tsconfigpaths from "vite-tsconfig-paths";
import { fileURLToPath } from "url"

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [tsconfigpaths(), svgr(), react()],
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
  }
})
