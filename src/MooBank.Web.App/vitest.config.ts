import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

// Standalone Vitest config (deliberately NOT importing vite.config.ts, whose top-level
// code generates HTTPS dev certificates via `dotnet dev-certs` — unavailable in CI).
export default defineConfig({
    plugins: [react()],
    test: {
        environment: "jsdom",
        globals: true,
        setupFiles: ["./src/test/setup.ts"],
        include: ["src/**/*.{test,spec}.{ts,tsx}"],
        css: false,
        coverage: {
            provider: "v8",
            reporter: ["text", "cobertura"],
            reportsDirectory: "./coverage",
            include: ["src/**/*.{ts,tsx}"],
            exclude: [
                "src/**/*.{test,spec}.{ts,tsx}",
                "src/test/**",
                "src/api/**",
                "src/routeTree.gen.ts",
                "src/**/*.d.ts",
            ],
        },
    },
    resolve: {
        // Mirror the app build (vite.config.ts): resolve the tsconfig `paths` mapping
        // `{ "*": ["./src/*"] }` so bare imports like "utils/currency" and "components" work.
        tsconfigPaths: true,
    },
});
